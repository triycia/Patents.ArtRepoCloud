using System.Reflection;
using Autofac;
using Patents.ArtRepoCloud.Domain.Code.Interfaces;
using Patents.ArtRepoCloud.Domain.Code;
using Patents.ArtRepoCloud.Domain.Configuration;
using Patents.ArtRepoCloud.Domain.Factories.Interfaces;
using Patents.ArtRepoCloud.Domain.Factories.ReferenceNumberFactories;
using Patents.ArtRepoCloud.Domain.Settings.EventBusSettings;
using Patents.ArtRepoCloud.Infrastructure;
using Patents.ArtRepoCloud.Infrastructure.Repositories;
using Vikcher.Framework.ApiCore.ApiMvcFilters;
using Vikcher.Framework.ApiCore.Behaviors;
using Vikcher.Framework.Azure.AppConfiguration;
using Vikcher.Framework.Common;
using Vikcher.Framework.EventBus.Abstractions.Queue;
using Vikcher.Framework.EventBus.AzureServiceBus.Queue;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Newtonsoft.Json;
using NLog.Extensions.Logging;
using Patents.ArtRepoCloud.Domain.Settings;
using Vikcher.Framework.IO.FileProxy;
using IdentityModel.AspNetCore.AccessTokenValidation;
using System.Security.Claims;
using Vikcher.Framework.Data.Cosmos;
using Patents.ArtRepoCloud.GraphService.Application.Commands.EnqueueBatchCommand;
using Patents.ArtRepoCloud.GraphService.Application.Validations;
using Patents.ArtRepoCloud.GraphService.Code.GraphQL.GraphQLTypes.ArtRepoCompanyTypes;
using Patents.ArtRepoCloud.GraphService.Code.GraphQL.GraphQLTypes.ArtRepoDocumentTypes;
using Patents.ArtRepoCloud.GraphService.Configuration;
using Patents.ArtRepoCloud.GraphService.GraphOperations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Patents.ArtRepoCloud.Domain.Interfaces;

namespace Patents.ArtRepoCloud.GraphService
{
    public static class WebApplicationBuilderExtensions
    {
        static WebApplicationBuilder _builder;
        static ILogger _logger;

        public static void AddBasics(this WebApplicationBuilder builder)
        {
            _builder = builder;
            using var loggerFactory =
                LoggerFactory.Create(builder =>
                {
                    builder.AddNLog();
                    builder.AddConsole();
                });
            _logger = loggerFactory.CreateLogger<Program>();

            builder.Host.ConfigureAppConfiguration(ConfigureAppConfiguration);

            ConfigureServices(builder);

            builder.Host.ConfigureContainer<Autofac.ContainerBuilder>(ConfigureContainer);
        }

        static void ConfigureEventBus(Autofac.ContainerBuilder builder)
        {
            _logger.LogInformation("Startup:ConfigureEventBus: configuring event bus");
            builder.RegisterAssemblyTypes(typeof(ArtRepoServiceBusHighPriorityQueueClientSettings).Assembly)
                .Where(t => t.IsClass && t.Namespace == "Patents.ArtRepoCloud.Domain.Settings.EventBusSettings")
                .AsSelf().SingleInstance();

            builder.RegisterGeneric(typeof(DefaultAzureServiceBusQueueClientFactory<>))
                .As(typeof(IAzureServiceBusQueueClientFactory<>)).SingleInstance();
            builder.RegisterGeneric(typeof(DefaultAzureServiceBusQueueClient<>)).As(typeof(IEventBusQueueClient<>))
                .SingleInstance();
            _logger.LogInformation("Startup:ConfigureEventBus: configured event bus");
        }

        static void ConfigureServices(WebApplicationBuilder builder)
        {
            _logger.LogInformation("Startup:ConfigureServices: configuring services");
            var services = builder.Services;

            services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
            services.AddScoped<IValidator<EnqueueBatchCommand>, EnqueueBatchCommandValidator>();
            
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            #region HotChocolate GraphQL

            builder.Services.AddGraphQLServer()
                .ModifyOptions(o =>
                {
                    o.EnableStream = true;
                })
                .AddType<ArtRepoDocumentType>()
                .AddType<CompanyDocumentFilterType>()
                .AddType<UploadType>()
                .AddMutationType(m => m.Name("Mutation"))
                .AddType<DocumentMutation>()
                .AddType<CompanyMutation>()
                .AddQueryType(q => q.Name("Query"))
                .AddType<DocumentQuery>()
                .AddType<CompanyQuery>()
                .AddProjections()
                .AddFiltering()
                .AddSorting()
                .AddMutationConventions();

            #endregion

            services.AddHttpClient("IPD.IFI.ApiService", c =>
            {
                var ifiApiSettings = GetIpdIfiApiSettings();
                c.BaseAddress = new Uri(ifiApiSettings.IpdIfiApiServiceUrl);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services
                .AddHttpContextAccessor();

            // Customize this value based on desired DNS refresh timer
            var socketsHttpHandler = new SocketsHttpHandler();
            socketsHttpHandler.PooledConnectionLifetime = TimeSpan.FromMinutes(5);
            services.AddSingleton(socketsHttpHandler);
            
            var serviceBusSettings = GetServiceBusSettings();

            services
                .AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddAzureServiceBusQueue(
                    serviceBusSettings.ConnectionString,
                    queueName: serviceBusSettings.ArtRepoQueue.HighPriority,
                    name: "artrepo-high-priority-queue-servicebus-check",
                    tags: new string[] { "servicebus" })
                .AddAzureServiceBusQueue(
                    serviceBusSettings.ConnectionString,
                    queueName: serviceBusSettings.ArtRepoQueue.NormalPriority,
                    name: "artrepo-normal-priority-queue-servicebus-check",
                    tags: new string[] { "servicebus" })
                .AddAzureServiceBusQueue(
                    serviceBusSettings.ConnectionString,
                    queueName: serviceBusSettings.ArtRepoQueue.LowPriority,
                    name: "artrepo-low-priority-queue-servicebus-check",
                    tags: new string[] { "servicebus" })
                .AddAzureServiceBusQueue(
                    serviceBusSettings.ConnectionString,
                    queueName: serviceBusSettings.ArtRepoQueue.IdlePriority,
                    name: "artrepo-idle-priority-queue-servicebus-check",
                    tags: new string[] { "servicebus" });

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorBehavior<,>));
            services.Configure<FormOptions>(o =>
            {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });

            services.AddEndpointsApiExplorer();

            services.AddFileApiProxy();

            ConfigureProtectedApiWithIdentityServer(services);

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddNLog();
                if(!Enum.TryParse(builder.Configuration["Logging:LogLevel:Default"], out LogLevel logLevel))
                {
                    return;
                }
                loggingBuilder.SetMinimumLevel(logLevel);
                if (builder.Environment.IsDevelopment() || logLevel == LogLevel.None) return;
                loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("", logLevel);
                loggingBuilder.AddApplicationInsights(builder.Configuration["ApplicationInsights:InstrumentationKey"]);
            });

            if (builder.Environment.EnvironmentName.StartsWith("Development", StringComparison.OrdinalIgnoreCase))
            {
                var globalExceptionFilterOptions = new GlobalExceptionFilterOptions() { IsDebug = true };
                services.AddSingleton(globalExceptionFilterOptions);
            }
            _logger.LogInformation("Startup:ConfigureServices: configured services");
        }

        static void ConfigureAppConfiguration(HostBuilderContext hostContext, IConfigurationBuilder config)
        {
            _logger.LogInformation("Startup:ConfigureAppConfiguration: configuring AzureAppConfiguration");
            config.AddJsonFile("appsettings.json", optional: true);
            config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                optional: true);

            var settings = config.Build();

            config.AddAzureAppConfiguration(options =>
            {
                if (hostContext.HostingEnvironment.IsDevelopment())
                {
                    AzureAppConfigurationHelper.ConfigureAzureAppConfiguration(
                        settings["ConnectionStrings:AzureAppConfiguration"],
                        settings["AzureAppConfigurationLabel"],
                        settings["AzureAppConfigurationKeys"],
                        options,
                        hostContext.HostingEnvironment.IsDevelopment()
                    );
                }
                else
                {
                    AzureAppConfigurationHelper.ConfigureAzureAppConfigurationWithManagedIdentity(
                        settings["ConnectionStrings:AzureAppConfiguration"],
                        settings["AzureAppConfigurationLabel"],
                        settings["AzureAppConfigurationKeys"],
                        options,
                        hostContext.HostingEnvironment.IsDevelopment()
                        , true
                    );
                }
            });
            _logger.LogInformation("Startup:ConfigureAppConfiguration: configured AzureAppConfiguration");
        }

        static void ConfigureContainer(HostBuilderContext hostBuilder, Autofac.ContainerBuilder containerBuilder)
        {
            _logger.LogInformation("Startup:ConfigureContainer: configuring container");
            containerBuilder.RegisterAssemblyModules(Assembly.Load("Vikcher.Framework.Common"));
            containerBuilder.RegisterAssemblyModules(Assembly.Load("Vikcher.Framework.Logging"));
            containerBuilder.RegisterAssemblyModules(Assembly.Load("Vikcher.Framework.Common"));
            containerBuilder.RegisterAssemblyModules(Assembly.Load("Vikcher.Framework.Data"));
            containerBuilder.RegisterAssemblyModules(Assembly.Load("Vikcher.Framework.Logging"));

            ConfigureAppSettings(containerBuilder);
            ConfigureEventBus(containerBuilder);

            containerBuilder.RegisterType<ReferenceNumberParser>().As<IReferenceNumberParser>().InstancePerLifetimeScope();
            containerBuilder.RegisterGeneric(typeof(ValidatorBehavior<,>)).As(typeof(IPipelineBehavior<,>));

            containerBuilder.RegisterType<HttpContextAccessor>().AsImplementedInterfaces().InstancePerLifetimeScope();

            #region Factories

            containerBuilder.RegisterType<UsApplicationReferenceNumberFactory>().AsSelf().As<IReferenceNumberFactory>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<UsPatentReferenceNumberFactory>().AsSelf().As<IReferenceNumberFactory>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<WipoReferenceNumberFactory>().AsSelf().As<IReferenceNumberFactory>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<EpoReferenceNumberFactory>().AsSelf().As<IReferenceNumberFactory>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<OtherReferenceNumberFactory>().AsSelf().As<IReferenceNumberFactory>().InstancePerLifetimeScope();

            containerBuilder.Register(c => new IReferenceNumberFactory[]
            {
                c.Resolve<UsApplicationReferenceNumberFactory>()
                ,c.Resolve<UsPatentReferenceNumberFactory>()
                ,c.Resolve<WipoReferenceNumberFactory>()
                ,c.Resolve<EpoReferenceNumberFactory>()
                ,c.Resolve<OtherReferenceNumberFactory>()
            }).InstancePerLifetimeScope();

            #endregion

            #region Infrastructure

            ConfigureDocumentDbContext(containerBuilder);
            ConfigureCompanyDbContext(hostBuilder, containerBuilder);

            containerBuilder.RegisterType<FileRepository>().AsImplementedInterfaces().InstancePerLifetimeScope();
            containerBuilder.RegisterType<CompanyRepository>().As<ICompanyRepository>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<DocumentRepository>().AsImplementedInterfaces().InstancePerLifetimeScope();

            #endregion
            _logger.LogInformation("Startup:ConfigureContainer: configured container");
        }

        public static void ConfigureDocumentDbContext(Autofac.ContainerBuilder builder)
        {
            _logger.LogInformation("Startup:ConfigureDocumentDbContext: configuring context");
            var settings = GetConnectionStrings();

            builder.Register(provider =>
            {
                var socketsHttpHandler = provider.Resolve<SocketsHttpHandler>();

                var cosmosClientOptions = new Microsoft.Azure.Cosmos.CosmosClientOptions()
                {
                    ConnectionMode = Microsoft.Azure.Cosmos.ConnectionMode.Gateway,
                    HttpClientFactory = () => new HttpClient(socketsHttpHandler, disposeHandler: false)
                };

                return new ContextOptions<DocumentDbContext>(settings.Database, settings.DatabaseName, cosmosClientOptions);
            }).AsSelf().SingleInstance();

            builder.RegisterType<ContextCosmosClientFactory<DocumentDbContext>>().As<IContextCosmosClientFactory<DocumentDbContext>>().SingleInstance();
            builder.RegisterType<DocumentDbContext>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType(typeof(UnitOfWork<DocumentDbContext>)).As(typeof(IUnitOfWork<DocumentDbContext>)).InstancePerLifetimeScope();
            _logger.LogInformation("Startup:ConfigureDocumentDbContext: configured context");
        }

        private static void ConfigureCompanyDbContext(HostBuilderContext hostBuilder, ContainerBuilder builder)
        {
            _logger.LogDebug($"Started configuring DbContext.");

            var settings = GetConnectionStrings();

            var dbContextOptions = new DbContextOptionsBuilder<CompanyDbContext>()
                .UseLazyLoadingProxies()
                .UseSqlServer(settings?.ArtRepoCompanies, options => options.EnableRetryOnFailure())
                .Options;

            builder.RegisterInstance(dbContextOptions).As<DbContextOptions<CompanyDbContext>>().SingleInstance();
            builder.Register(c =>
            {
                var dbContext = new CompanyDbContext(c.Resolve<DbContextOptions<CompanyDbContext>>(), c.Resolve<IMediator>());
                
                if (hostBuilder.HostingEnvironment.IsDevelopment()) return dbContext;

                if (!(dbContext.Database.GetDbConnection() is SqlConnection dbConn))
                    throw new InvalidOperationException(
                        $"Sql connection is not found when resolving the {nameof(CompanyDbContext)}.");

                dbConn.AccessToken = (new Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider())
                    .GetAccessTokenAsync(_builder.Configuration["AzureDatabaseTokenEndpoint"]).Result;

                return dbContext;
            }).AsImplementedInterfaces().AsSelf().InstancePerLifetimeScope();
        }

        static void ConfigureAppSettings(Autofac.ContainerBuilder containerBuilder)
        {
            _logger.LogInformation("Startup:ConfigureAppSettings: configuring app settings");
            var connectionStrings = GetConnectionStrings();
            var serviceBusSettings = GetServiceBusSettings();
            var directorySettings = GetDirectorySettings();
            var ifiApiSettings = GetIpdIfiApiSettings();

            var obj = new
            {
                connectionStrings,
                directorySettings,
                serviceBusSettings,
                ifiApiSettings
            };

            _logger.LogInformation($"Settings: {obj.ToJson()}");

            containerBuilder.RegisterInstance(connectionStrings).As<ConnectionStrings>().SingleInstance();
            containerBuilder.RegisterInstance(serviceBusSettings).As<ServiceBusSettings>().SingleInstance();
            containerBuilder.RegisterInstance(directorySettings).As<IDirectorySettings>().SingleInstance();
            containerBuilder.RegisterInstance(ifiApiSettings).As<IpdIfiApiSettings>().SingleInstance();
            _logger.LogInformation("Startup:ConfigureAppSettings: configured app settings");
        }

        static void ConfigureProtectedApiWithIdentityServer(IServiceCollection services)
        {
            _logger.LogInformation("Startup:ConfigureProtectedApiWithIdentityServer: configuring JwtBearerOptions");
            services.AddAuthentication("token")
                    .AddJwtBearer("token", options =>
                    {
                        // base-address of your identityserver
                        options.Authority = _builder.Configuration["IdentityServerRegistration:Authority"];

                        // name of the API resource
                        options.Audience = _builder.Configuration["IdentityServerRegistration:Audience"];
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters.ClockSkew = new TimeSpan(0, 0, 0);
                        options.TokenValidationParameters.ValidIssuer =
                            _builder.Configuration["IdentityServerRegistration:TokenIssuerUri"];

                        // if token does not contain a dot, it is a reference token
                        options.ForwardDefaultSelector = Selector.ForwardReferenceToken("introspection");
                    })
                    .AddOAuth2Introspection("introspection", options =>
                    {
                        // base-address of your identityserver
                        options.Authority = _builder.Configuration["IdentityServerRegistration:Authority"];

                        options.ClientId = _builder.Configuration["IdentityServerRegistration:ClientId"];
                        options.ClientSecret = _builder.Configuration["IdentityServerRegistration:ClientSecret"];

                        options.DiscoveryPolicy.Authority = _builder.Configuration["IdentityServerRegistration:TokenIssuerUri"];
                        options.DiscoveryPolicy.RequireHttps = Convert.ToBoolean(_builder.Configuration["IdentityServerRegistration:DiscoveryPolicy:RequireHttps"]);
                        options.DiscoveryPolicy.ValidateEndpoints = Convert.ToBoolean(_builder.Configuration["IdentityServerRegistration:DiscoveryPolicy:ValidateEndpoints"]);
                        options.RoleClaimType = ClaimTypes.Role;
                        options.NameClaimType = ClaimTypes.Name;
                    });

            _logger.LogInformation("Startup:ConfigureProtectedApiWithIdentityServer: configured JwtBearerOptions");
        }

        private static ServiceBusSettings GetServiceBusSettings()
        {
            var result = _builder.Configuration.GetSection("ServiceBus").Get<ServiceBusSettings>();
            _logger.LogDebug($"{nameof(ServiceBusSettings)}: {result.ToJson()}");

            return result;
        }

        private static ConnectionStrings? GetConnectionStrings()
        {
            var result = _builder.Configuration.GetSection(nameof(ConnectionStrings)).Get<ConnectionStrings>();
            _logger.LogDebug($"{nameof(ConnectionStrings)}: {JsonConvert.SerializeObject(result)}");
            return result;
        }

        private static IpdIfiApiSettings? GetIpdIfiApiSettings()
        {
            var result = _builder.Configuration.GetSection(nameof(IpdIfiApiSettings)).Get<IpdIfiApiSettings>();
            _logger.LogDebug($"{nameof(IpdIfiApiSettings)}: {result.ToJson()}");
            return result;
        }

        private static DirectorySettings? GetDirectorySettings()
        {
            var result = _builder.Configuration.GetSection(nameof(DirectorySettings)).Get<DirectorySettings>();
            _logger.LogDebug($"{nameof(DirectorySettings)}: {result.ToJson()}");
            return result;
        }
    }
}