FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0.200 AS build

WORKDIR /src
COPY . .
RUN dotnet tool restore
ENV PATH="/root/.dotnet/tools:${PATH}"

WORKDIR /src
COPY . .
RUN dotnet tool restore
ENV PATH="/root/.dotnet/tools:${PATH}"

WORKDIR /src/Server
RUN dotnet paket restore
RUN dotnet restore -nowarn:msb3202,nu1503
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final

WORKDIR /app
COPY --from=publish /app .

ENTRYPOINT ["dotnet", "KafkaSimpleDashboard.Server.dll"]