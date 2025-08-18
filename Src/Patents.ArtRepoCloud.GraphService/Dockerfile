#Define global ARGs
ARG SDK_VERSION=6.0.404-alpine3.17
ARG RUNTIME_VERSION=6.0.12-alpine3.17
ARG APP_NAME="Patents.ArtRepoCloud.GraphService"

FROM mcr.microsoft.com/dotnet/sdk:$SDK_VERSION AS build

#Pass global ARG into current stage
ARG APP_NAME
#Define stage ARG
ARG PROJECT_PATH="$APP_NAME/$APP_NAME.csproj"

WORKDIR /src

# add bash
RUN apk add --no-cache bash
# add curl
RUN apk add --no-cache curl
# nuget restore
# Install Credential Provider and set env variables to enable Nuget restore with auth
ARG PAT
RUN wget -qO- https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | bash
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED true
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS "{\"endpointCredentials\": {YOUR_ENDPOINT_CRED}}"

COPY . .
RUN dotnet restore $PROJECT_PATH -r linux-musl-x64 /p:PublishTrimmed=true /p:PublishReadyToRun=true
RUN dotnet publish $PROJECT_PATH -c release -o /app/publish -r linux-musl-x64 --self-contained true --no-restore /p:PublishTrimmed=true /p:PublishReadyToRun=true


# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:$RUNTIME_VERSION

#Pass global ARG into current stage
ARG APP_NAME

#Pass global ARG into current stage
ARG APP_NAME

# The user the app should run as
ENV APP_USER=app
# The home directory
ENV APP_DIR="/$APP_USER"
# default directory is /app
WORKDIR $APP_DIR


# See: https://github.com/dotnet/announcements/issues/20
# Uncomment to enable globalization APIs (or delete)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT false
RUN apk add --no-cache icu-libs
ENV LC_ALL en_US.UTF-8
ENV LANG en_US.UTF-8

# Harden docker image
COPY --from=build /src/$APP_NAME/HardeningAlpine.sh .
RUN chmod +x HardeningAlpine.sh && \
  sh HardeningAlpine.sh && \
  rm HardeningAlpine.sh

# Run some post install hardening commands
COPY --from=build /src/$APP_NAME/PostInstallLinux.sh .
RUN chmod +x PostInstallLinux.sh && \
  sh PostInstallLinux.sh $APP_NAME && \
  rm PostInstallLinux.sh

# Run app as non root user
USER $APP_USER

#OS restriction: You need NET_ADMIN /root to bind on "reserved ports < 1024
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
COPY --from=build /app/publish ./

ENTRYPOINT ["./Patents.ArtRepoCloud.GraphService"]
