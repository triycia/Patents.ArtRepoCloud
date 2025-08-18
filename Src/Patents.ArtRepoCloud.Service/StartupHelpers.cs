using System.Net;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Vikcher.Framework.Common;
using Vikcher.Framework.EventBus.Abstractions.Queue;
using Patents.ArtRepoCloud.Domain.IntegrationEvents;
using Patents.ArtRepoCloud.Infrastructure;
using Patents.ArtRepoCloud.Infrastructure.Repositories;
using Patents.ArtRepoCloud.Service.Code.AzureServiceBusQueue;
using Patents.ArtRepoCloud.Service.Configuration;
using Patents.ArtRepoCloud.Service.DataFetchers;
using Patents.ArtRepoCloud.Service.EventBusSettings;
using Patents.ArtRepoCloud.Service.Extensions;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Patents.ArtRepoCloud.Service.DataProviders.Epo;
using Patents.ArtRepoCloud.Service.DataProviders.Epo.Interfaces;
using Patents.ArtRepoCloud.Service.DataProviders.Ifi;
using Patents.ArtRepoCloud.Service.DataProviders.Ifi.Interfaces;
using Patents.ArtRepoCloud.Domain.Configuration;
using Patents.ArtRepoCloud.Domain.Settings;
using Patents.ArtRepoCloud.Service.DataProviders.IpdIfi;
using Patents.ArtRepoCloud.Service.DataProviders.Questel;
using Patents.ArtRepoCloud.Service.DataProviders.Questel.Interfaces;
using Patents.ArtRepoCloud.Service.DataProviders.IpdIfi.Interfaces;
using Patents.ArtRepoCloud.Service.DataFetchers.Epo;
using Patents.ArtRepoCloud.Service.DataFetchers.Ifi;
using Patents.ArtRepoCloud.Service.DataFetchers.IpdIfi;
using Patents.ArtRepoCloud.Service.DataFetchers.Questel;
using Patents.ArtRepoCloud.Domain.Settings.EventBusSettings;
using Vikcher.Framework.EventBus.AzureServiceBus.Queue;
using Patents.ArtRepoCloud.Domain.Code.Interfaces;
using Patents.ArtRepoCloud.Domain.Code;
using Patents.ArtRepoCloud.Domain.Factories.Interfaces;
using Patents.ArtRepoCloud.Domain.Factories.ReferenceNumberFactories;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchResponses;
using Patents.ArtRepoCloud.Service.DataFetchers.FetchRequests;
using Patents.ArtRepoCloud.Service.DataFetchers.Uspto;
using Patents.ArtRepoCloud.Service.DataProviders.Uspto;
using Polly;
using Patents.ArtRepoCloud.Service.DataProviders.Uspto.Interfaces;
using Patents.ArtRepoCloud.Service.Services;
using Patents.ArtRepoCloud.Service.Services.Interfaces;
using Vikcher.Framework.IO.FileProxy;
using Patents.ArtRepoCloud.Service.Code.AzureServiceBusQueue.Interfaces;
using Patents.ArtRepoCloud.Service.EventBusSettings.Interfaces;
using Vikcher.Framework.Data.Cosmos;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Patents.ArtRepoCloud.Service
{
    public static class StartupHelpers
    {
        private static readonly NLog.ILogger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            Guard.AssertNotNull(Logger, nameof(Logger));

            Logger.Info("StartupHelpers: ConfigureServices started.");

            services.AddAutofac();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(StartupHelpers).Assembly));

            services.AddHttpClient("IPD.IFI.ApiService", c =>
            {
                var ifiApiSettings = GetIpdIfiApiSettings(hostContext);
                c.BaseAddress = new Uri(ifiApiSettings.IpdIfiApiServiceUrl);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddHttpClient("IficlaimsService", c =>
            {
                var settings = GetIficlaimsSettings(hostContext);
                c.BaseAddress = new Uri(settings.IfiServiceUrl);
                c.DefaultRequestHeaders.Add("x-user", settings.IfiUserName);
                c.DefaultRequestHeaders.Add("x-password", settings.IfiPassword);
            });

            services.AddHttpClient("EpoService", c =>
            {
                var epoSettings = GetEpoSettings(hostContext);
                c.BaseAddress = new Uri(epoSettings.EpoServiceUrl);
                c.DefaultRequestHeaders.TryAddWithoutValidation("Content-Source", "application/x-www-form-urlencoded");
            });

            services.AddHttpClient("QuestelService", c =>
            {
                var questelSettings = GetQuestelSettings(hostContext);
                c.BaseAddress = new Uri(questelSettings.QuestelUrl);
            });

            services.AddHttpClient("UsptoService", (service, client) =>
            {
                client.BaseAddress = new Uri("http://appft.uspto.gov");
            }).AddPolicyHandler(GetUsptoRetryPolicy());

            services.AddHttpClient("PatentCenter", (service, client) =>
            {
                client.BaseAddress = new Uri("https://patentcenter.uspto.gov/");
            }).AddPolicyHandler(GetUsptoRetryPolicy());

            services.AddHostedService<Worker>();

            var db = GetConnectionStrings(hostContext);

            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy());
                //.AddCosmosDb(db.Database,
                //    db.DatabaseName,
                //    name: "todo-db-check",
                //    failureStatus: HealthStatus.Unhealthy,
                //    tags: new[] { "todo-service", "cosmosdb" });

            services.AddFileApiProxy();

            // Customize this value based on desired DNS refresh timer
            var socketsHttpHandler = new SocketsHttpHandler();
            socketsHttpHandler.PooledConnectionLifetime = TimeSpan.FromMinutes(5);
            services.AddSingleton(socketsHttpHandler);

            Logger.Info("StartupHelpers: ConfigureServices completed.");
        }

        internal static void ConfigureContainer(HostBuilderContext hostContext, ContainerBuilder builder)
        {
            Guard.AssertNotNull(Logger, nameof(Logger));

            Logger.Info("StartupHelpers: ConfigureContainer started.");
            ConfigureAppSettings(hostContext, builder);

            var configuration = MediatRConfigurationBuilder
                .Create(typeof(ArtRepoDocumentIntegrationEvent).Assembly)
                .WithAllOpenGenericHandlerTypesRegistered()
                .WithRegistrationScope(RegistrationScope.Scoped)
                .Build();

            builder.RegisterMediatR(configuration);

            builder.RegisterAssemblyModules(Assembly.Load("Vikcher.Framework.Common"));
            builder.RegisterAssemblyModules(Assembly.Load("Vikcher.Framework.Data"));
            builder.RegisterAssemblyModules(Assembly.Load("Vikcher.Framework.Logging"));
            builder.RegisterAssemblyModules(Assembly.Load("Vikcher.Framework.EventBus"));

            #region Infrastructure

            ConfigureDocumentDbContext(hostContext, builder);
            ConfigureCompanyDbContext(hostContext, builder);
            builder.RegisterType<DocumentRepository>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<CompanyRepository>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<FileRepository>().AsImplementedInterfaces().InstancePerLifetimeScope();

            #endregion

            builder.RegisterType<IpdIfiApiProxy>().AsSelf().As<IIpdIfiApiProxy>().InstancePerLifetimeScope();
            builder.RegisterType<IfiApiProxy>().AsSelf().As<IIfiApiProxy>().InstancePerLifetimeScope();
            builder.RegisterType<EpoApiProxy>().AsSelf().As<IEpoApiProxy>().InstancePerLifetimeScope();
            builder.RegisterType<QuestelApiProxy>().AsSelf().As<IQuestelApiProxy>().InstancePerLifetimeScope();
            builder.RegisterType<UsptoApiProxy>().AsSelf().As<IUsptoApiProxy>().InstancePerLifetimeScope();

            #region Fetchers

            builder.RegisterType<PairDocumentFetcher>().AsSelf().As<IFetcher<PairDocumentFetchRequest, PairDocumentFetchResponse>>().InstancePerLifetimeScope();
            builder.RegisterType<PairPdfFetcher>().AsSelf().As<IFetcher<PairPdfFetchRequest, PairPdfFetchResponse>>().InstancePerLifetimeScope();

            var fetcherSettings = GetFetcherSettings(hostContext);

            builder.RegisterTypes(typeof(IpdIfiDocumentFetcher), typeof(IfiDocumentFetcher), typeof(EpoDocumentFetcher), typeof(PairDocumentFetcher))
                .Where(t => fetcherSettings.IsFetcherEnabled(t))
                .As<IFetcher<DocumentFetchRequest, DocumentFetchResponse>>()
                .InstancePerLifetimeScope();

            builder.RegisterTypes(typeof(IfiPdfFetcher), typeof(EpoPdfFetcher), typeof(PairPdfFetcher), typeof(QuestelPdfFetcher))
                .Where(t => fetcherSettings.IsFetcherEnabled(t))
                .As<IFetcher<PdfFetchRequest, PdfFetchResponse>>()
                .InstancePerLifetimeScope();

            builder.RegisterTypes(typeof(IpdIfiImageFetcher), typeof(IfiImageFetcher))
                .Where(t => fetcherSettings.IsFetcherEnabled(t))
                .As<IFetcher<ImageFetchRequest, ImageFetchResponse>>()
                .InstancePerLifetimeScope();

            #endregion

            #region Factories
            builder.RegisterType<UsApplicationReferenceNumberFactory>().AsSelf().As<IReferenceNumberFactory>().InstancePerLifetimeScope();
            builder.RegisterType<UsPatentReferenceNumberFactory>().AsSelf().As<IReferenceNumberFactory>().InstancePerLifetimeScope();
            builder.RegisterType<WipoReferenceNumberFactory>().AsSelf().As<IReferenceNumberFactory>().InstancePerLifetimeScope();
            builder.RegisterType<EpoReferenceNumberFactory>().AsSelf().As<IReferenceNumberFactory>().InstancePerLifetimeScope();
            builder.RegisterType<OtherReferenceNumberFactory>().AsSelf().As<IReferenceNumberFactory>().InstancePerLifetimeScope();

            builder.Register(c => new IReferenceNumberFactory[]
            {
                c.Resolve<UsApplicationReferenceNumberFactory>()
                ,c.Resolve<UsPatentReferenceNumberFactory>()
                ,c.Resolve<WipoReferenceNumberFactory>()
                ,c.Resolve<EpoReferenceNumberFactory>()
                ,c.Resolve<OtherReferenceNumberFactory>()
            }).InstancePerLifetimeScope();

            builder.RegisterType<ReferenceNumberParser>().As<IReferenceNumberParser>().InstancePerLifetimeScope();

            #endregion

            #region Services

            builder.RegisterType<AwsClientService>().As<IAwsClientService>().InstancePerLifetimeScope();

            #endregion

            ConfigureEventBus(builder);

            Logger.Info("StartupHelpers: ConfigureContainer completed.");
        }

        static void ConfigureEventBus(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(ArtRepoServiceBusHighPriorityQueueSettings).Assembly)
                .Where(t => t.IsClass 
                            && t.Namespace == "Patents.ArtRepoCloud.Service.EventBusSettings" 
                            && t.GetInterface(nameof(IPriorityEventBusQueueSettings)) != null)
                .AsSelf().SingleInstance();

            builder.RegisterGeneric(typeof(ArtRepoAzureServiceBusQueue<>)).As(typeof(IPriorityEventBusQueue<>)).SingleInstance();

            builder.RegisterType<ArtRepoFileServiceBusQueueSettings>().AsSelf().SingleInstance();
            builder.RegisterGeneric(typeof(DefaultAzureServiceBusQueue<>)).As(typeof(IEventBusQueue<>)).SingleInstance();

            builder.RegisterAssemblyTypes(typeof(ArtRepoServiceBusHighPriorityQueueClientSettings).Assembly)
                .Where(t => t.IsClass && t.Namespace == "Patents.ArtRepoCloud.Domain.Settings.EventBusSettings")
                .AsSelf().SingleInstance();

            builder.RegisterGeneric(typeof(DefaultAzureServiceBusQueueClientFactory<>))
                .As(typeof(IAzureServiceBusQueueClientFactory<>)).SingleInstance();
            builder.RegisterGeneric(typeof(DefaultAzureServiceBusQueueClient<>)).As(typeof(IEventBusQueueClient<>))
                .SingleInstance();

            Logger.Info("Startup: registered Event Bus in Ioc container.");
        }

        static void ConfigureAppSettings(HostBuilderContext hostContext, ContainerBuilder builder)
        {
            Logger.Debug($"Started configuring app settings");

            var connectionStrings = GetConnectionStrings(hostContext);
            var appSettings = GetAppSettings(hostContext);
            var directorySettings = GetDirectorySettings(hostContext);
            var ifiApiSettings = GetIpdIfiApiSettings(hostContext);
            var epoSettings = GetEpoSettings(hostContext);
            var ificlaimsSettings = GetIficlaimsSettings(hostContext);
            var questelSettings = GetQuestelSettings(hostContext);
            var usptoApiSettings = GetUsptoApiSettings(hostContext);
            var pairSettings = GetPairSettings(hostContext);
            var serviceBusSettings = GetServiceBusSettings(hostContext);
            var awsSettings = GetAwsSettings(hostContext);
            var awsInstanceSettings = GetAwsInstanceSettings(hostContext);

            var obj = new
            {
                connectionStrings,
                appSettings,
                directorySettings,
                ifiApiSettings,
                epoSettings,
                ificlaimsSettings,
                questelSettings,
                usptoApiSettings,
                pairSettings,
                serviceBusSettings,
                awsSettings,
                awsInstanceSettings
            };

            Logger.Info($"Settings: {obj.ToJson()}");

            builder.RegisterInstance(connectionStrings).AsSelf().SingleInstance();
            builder.RegisterInstance(appSettings).AsSelf().SingleInstance();
            builder.RegisterInstance(directorySettings).As<IDirectorySettings>().SingleInstance();
            builder.RegisterInstance(ifiApiSettings).As<IpdIfiApiSettings>().SingleInstance();
            builder.RegisterInstance(epoSettings).As<EpoSettings>().SingleInstance();
            builder.RegisterInstance(ificlaimsSettings).As<IficlaimsSettings>().SingleInstance();
            builder.RegisterInstance(questelSettings).As<QuestelSettings>().SingleInstance();
            builder.RegisterInstance(usptoApiSettings).As<UsptoApiSettings>().SingleInstance();
            builder.RegisterInstance(pairSettings).As<PairSettings>().SingleInstance();
            builder.RegisterInstance(serviceBusSettings).AsSelf().SingleInstance();
            builder.RegisterInstance(awsSettings).As<AwsSettings>().SingleInstance();
            builder.RegisterInstance(awsInstanceSettings).As<AwsInstanceSettings>().SingleInstance();
        }

        public static void ConfigureDocumentDbContext(HostBuilderContext hostContext, ContainerBuilder builder)
        {
            var settings = GetConnectionStrings(hostContext);

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
        }

        private static void ConfigureCompanyDbContext(HostBuilderContext hostContext, ContainerBuilder builder)
        {
            Logger.Debug($"Started configuring DbContext.");

            var settings = GetConnectionStrings(hostContext);

            var ruleDbContextOptions = new DbContextOptionsBuilder<CompanyDbContext>()
                .UseLazyLoadingProxies()
                .UseSqlServer(settings?.ArtRepoCompanies, options => options.EnableRetryOnFailure())
                .Options;
            builder.RegisterInstance(ruleDbContextOptions).As<DbContextOptions<CompanyDbContext>>().SingleInstance();
            builder.Register(c =>
            {
                var dbContext = new CompanyDbContext(c.Resolve<DbContextOptions<CompanyDbContext>>(), c.Resolve<IMediator>());

                if (hostContext.HostingEnvironment.IsDevelopment()) return dbContext;

                if (!(dbContext.Database.GetDbConnection() is SqlConnection dbConn))
                    throw new InvalidOperationException(
                        $"Sql connection is not found when resolving the {nameof(CompanyDbContext)}.");

                dbConn.AccessToken = (new Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider())
                    .GetAccessTokenAsync(hostContext.Configuration["AzureDatabaseTokenEndpoint"]).Result;

                return dbContext;
            }).AsImplementedInterfaces().AsSelf().InstancePerLifetimeScope();
        }

        static IAsyncPolicy<HttpResponseMessage> GetUsptoRetryPolicy()
        {
                return Policy<HttpResponseMessage>.Handle<HttpRequestException>()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .CipWaitAndRetryAsync(3, (retryAttempt, response, context) =>
                {
                    var seconds = response.Result.Headers.TryGetValue<int>("RetryCount-After");

                    return TimeSpan.FromSeconds(seconds > 0 ? seconds * retryAttempt : 10);
                });
        }

        #region Setting

        static ConnectionStrings? GetConnectionStrings(HostBuilderContext hostContext)
        {
            var result = hostContext.Configuration.GetSection(nameof(ConnectionStrings)).Get<ConnectionStrings>();
            Logger.Debug($"ConnectionStrings: {result.ToJson()}");
            return result;
        }

        static AppSettings? GetAppSettings(HostBuilderContext hostContext)
        {
            var result = hostContext.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
            Logger.Debug($"AppSettings: {result.ToJson()}");
            return result;
        }

        private static DirectorySettings? GetDirectorySettings(HostBuilderContext hostContext)
        {
            var result = hostContext.Configuration.GetSection(nameof(DirectorySettings)).Get<DirectorySettings>();
            Logger.Debug($"{nameof(DirectorySettings)}: {result.ToJson()}");
            return result;
        }

        static IpdIfiApiSettings? GetIpdIfiApiSettings(HostBuilderContext hostContext)
        {
            var result = hostContext.Configuration.GetSection(nameof(IpdIfiApiSettings)).Get<IpdIfiApiSettings>();
            Logger.Debug($"{nameof(IpdIfiApiSettings)}: {result.ToJson()}");
            return result;
        }

        static EpoSettings? GetEpoSettings(HostBuilderContext hostContext)
        {
            var result = hostContext.Configuration.GetSection(nameof(EpoSettings)).Get<EpoSettings>();
            Logger.Debug($"{nameof(EpoSettings)}: {result.ToJson()}");
            return result;
        }

        static IficlaimsSettings? GetIficlaimsSettings(HostBuilderContext hostContext)
        {
            var result = hostContext.Configuration.GetSection(nameof(IficlaimsSettings)).Get<IficlaimsSettings>();
            Logger.Debug($"{nameof(IficlaimsSettings)}: {result.ToJson()}");
            return result;
        }

        static QuestelSettings? GetQuestelSettings(HostBuilderContext hostContext)
        {
            var result = hostContext.Configuration.GetSection(nameof(QuestelSettings)).Get<QuestelSettings>();
            Logger.Debug($"{nameof(QuestelSettings)}: {result.ToJson()}");
            return result;
        }

        static UsptoApiSettings? GetUsptoApiSettings(HostBuilderContext hostContext)
        {
            var result = hostContext.Configuration.GetSection(nameof(UsptoApiSettings)).Get<UsptoApiSettings>();
            Logger.Debug($"{nameof(UsptoApiSettings)}: {result.ToJson()}");
            return result;
        }

        static PairSettings? GetPairSettings(HostBuilderContext hostContext)
        {
            var result = hostContext.Configuration.GetSection(nameof(PairSettings)).Get<PairSettings>();
            Logger.Debug($"{nameof(PairSettings)}: {result.ToJson()}");
            return result;
        }

        static FetcherSettings GetFetcherSettings(HostBuilderContext hostContext)
        {
            var result = hostContext.Configuration.GetSection(nameof(FetcherSettings)).Get<FetcherSettings>();
            Logger.Debug($"FetcherSettings: {result.ToJson()}");
            return result;
        }

        static ServiceBusSettings? GetServiceBusSettings(HostBuilderContext hostContext)
        {
            var result = hostContext.Configuration.GetSection("ServiceBus").Get<ServiceBusSettings>();
            Logger.Debug($"ServiceBus: {result.ToJson()}");
            return result;
        }

        static AwsSettings? GetAwsSettings(HostBuilderContext hostContext)
        {
            var result = hostContext.Configuration.GetSection(nameof(AwsSettings)).Get<AwsSettings>();
            Logger.Debug($"{nameof(AwsSettings)}: {result.ToJson()}");
            return result;
        }

        static AwsInstanceSettings? GetAwsInstanceSettings(HostBuilderContext hostContext)
        {
            var result = hostContext.Configuration.GetSection(nameof(AwsInstanceSettings)).Get<AwsInstanceSettings>();
            Logger.Debug($"{nameof(AwsInstanceSettings)}: {result.ToJson()}");
            return result;
        }

        #endregion
    }
}