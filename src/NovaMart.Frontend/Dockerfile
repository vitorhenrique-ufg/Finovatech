FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY src/NovaMart.Frontend/NovaMart.Frontend.csproj NovaMart.Frontend/
RUN dotnet restore NovaMart.Frontend/NovaMart.Frontend.csproj
COPY src/NovaMart.Frontend/ NovaMart.Frontend/
WORKDIR /src/NovaMart.Frontend
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NovaMart.Frontend.dll"]
