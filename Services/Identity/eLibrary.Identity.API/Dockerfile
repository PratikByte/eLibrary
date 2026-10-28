FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore Services/Identity/eLibrary.Identity.API/eLibrary.Identity.API.csproj
RUN dotnet publish Services/Identity/eLibrary.Identity.API/eLibrary.Identity.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

RUN dotnet tool install dotnet-ef --tool-path /tools --version 8.0.4
RUN mkdir -p /app/migrations \
    && /tools/dotnet-ef migrations bundle \
        --project Services/Identity/eLibrary.Identity.Infrastructure/eLibrary.Identity.Infrastructure.csproj \
        --startup-project Services/Identity/eLibrary.Identity.API/eLibrary.Identity.API.csproj \
        --context IdentityDbContext \
        --configuration Release \
        --output /app/migrations/efbundle \
    && cp /app/publish/appsettings*.json /app/migrations/ \
    && chmod +x /app/migrations/efbundle

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish/ .
COPY --from=build /app/migrations/ ./migrations/

ENV ASPNETCORE_URLS=http://+:8001
EXPOSE 8001

ENTRYPOINT ["dotnet", "eLibrary.Identity.API.dll"]
