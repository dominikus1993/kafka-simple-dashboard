FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.401 AS build
WORKDIR /src
COPY . .
RUN dotnet tool restore
ENV PATH="/root/.dotnet/tools:${PATH}"

WORKDIR /src/Server
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "KafkaSimpleDashboard.Server.dll"]