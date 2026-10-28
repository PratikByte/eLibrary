FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore Services/Book/eLibrary.Book.API/eLibrary.Book.API.csproj
RUN dotnet publish Services/Book/eLibrary.Book.API/eLibrary.Book.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

RUN dotnet tool install dotnet-ef --tool-path /tools --version 8.0.4
RUN mkdir -p /app/migrations \
    && /tools/dotnet-ef migrations bundle \
        --project Services/Book/eLibrary.Book.Infrastructure/eLibrary.Book.Infrastructure.csproj \
        --startup-project Services/Book/eLibrary.Book.API/eLibrary.Book.API.csproj \
        --context BookDbContext \
        --configuration Release \
        --output /app/migrations/efbundle \
    && cp /app/publish/appsettings*.json /app/migrations/ \
    && chmod +x /app/migrations/efbundle

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish/ .
COPY --from=build /app/migrations/ ./migrations/

ENV ASPNETCORE_URLS=http://+:8002
EXPOSE 8002

ENTRYPOINT ["dotnet", "eLibrary.Book.API.dll"]
