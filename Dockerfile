FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
RUN apt update && apt install -y git
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RepositoryLinter/RepositoryLinter.csproj", "RepositoryLinter/"]
RUN dotnet restore "RepositoryLinter/RepositoryLinter.csproj"
COPY . .
WORKDIR "/src/RepositoryLinter"
RUN dotnet build "RepositoryLinter.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RepositoryLinter.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RepositoryLinter.dll"]
