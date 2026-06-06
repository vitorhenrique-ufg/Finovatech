FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY src/Finovatech.GatewayApi/Finovatech.GatewayApi.csproj Finovatech.GatewayApi/
RUN dotnet restore Finovatech.GatewayApi/Finovatech.GatewayApi.csproj
COPY src/Finovatech.GatewayApi/ Finovatech.GatewayApi/
WORKDIR /src/Finovatech.GatewayApi
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Finovatech.GatewayApi.dll"]
