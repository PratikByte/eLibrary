FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore Services/Gateway/eLibrary.Gateway.API/eLibrary.Gateway.API.csproj
RUN dotnet publish Services/Gateway/eLibrary.Gateway.API/eLibrary.Gateway.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish/ .

ENV ASPNETCORE_URLS=http://+:8000
EXPOSE 8000

ENTRYPOINT ["dotnet", "eLibrary.Gateway.API.dll"]
