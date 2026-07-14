FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CleanApiStarter.sln global.json Directory.Build.props dotnet-tools.json ./
COPY src/CleanApiStarter.Domain/CleanApiStarter.Domain.csproj src/CleanApiStarter.Domain/
COPY src/CleanApiStarter.Application/CleanApiStarter.Application.csproj src/CleanApiStarter.Application/
COPY src/CleanApiStarter.Infrastructure/CleanApiStarter.Infrastructure.csproj src/CleanApiStarter.Infrastructure/
COPY src/CleanApiStarter.Api/CleanApiStarter.Api.csproj src/CleanApiStarter.Api/
COPY tests/CleanApiStarter.Tests/CleanApiStarter.Tests.csproj tests/CleanApiStarter.Tests/
RUN dotnet restore src/CleanApiStarter.Api/CleanApiStarter.Api.csproj

COPY src/ src/
RUN dotnet publish src/CleanApiStarter.Api/CleanApiStarter.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CleanApiStarter.Api.dll"]
