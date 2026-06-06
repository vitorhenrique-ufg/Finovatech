FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY src/Finovatech.Contratos/Finovatech.Contratos.csproj Finovatech.Contratos/
COPY src/Finovatech.ServicoNotificacao/Finovatech.ServicoNotificacao.csproj Finovatech.ServicoNotificacao/
RUN dotnet restore Finovatech.ServicoNotificacao/Finovatech.ServicoNotificacao.csproj
COPY src/Finovatech.Contratos/ Finovatech.Contratos/
COPY src/Finovatech.ServicoNotificacao/ Finovatech.ServicoNotificacao/
WORKDIR /src/Finovatech.ServicoNotificacao
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Finovatech.ServicoNotificacao.dll"]
