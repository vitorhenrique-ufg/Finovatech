FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY src/Finovatech.ServicoCatalogo/Finovatech.ServicoCatalogo.csproj Finovatech.ServicoCatalogo/
RUN dotnet restore Finovatech.ServicoCatalogo/Finovatech.ServicoCatalogo.csproj
COPY src/Finovatech.ServicoCatalogo/ Finovatech.ServicoCatalogo/
WORKDIR /src/Finovatech.ServicoCatalogo
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Finovatech.ServicoCatalogo.dll"]
