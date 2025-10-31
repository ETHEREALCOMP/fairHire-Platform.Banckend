FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY fairHire.slnx .
COPY FairHire.API/FairHire.API.csproj ./FairHire.API/
COPY FairHire.Application/FairHire.Application.csproj ./FairHire.Application/
COPY FairHire.Domain/FairHire.Domain.csproj ./FairHire.Domain/
COPY FairHire.Infrastructure.Postgres/FairHire.Infrastructure.Postgres.csproj ./FairHire.Infrastructure.Postgres/

RUN dotnet restore "fairHire.slnx"

COPY . .

WORKDIR /src/FairHire.API
RUN dotnet publish "FairHire.API.csproj" -c Release -o /app/publish

RUN dotnet tool install --global dotnet-ef --version 9.0.0

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime_base
WORKDIR /app

COPY --from=build /app/publish .

COPY --from=build /root/.dotnet/tools /root/.dotnet/tools
ENV PATH="$PATH:/root/.dotnet/tools"
ENV ASPNETCORE_URLS=http://+:8080

FROM runtime_base AS api
ENTRYPOINT ["dotnet", "FairHire.API.dll"]

FROM build AS db-migrator
WORKDIR /src/FairHire.API

ENV PATH="$PATH:/root/.dotnet/tools"

ENTRYPOINT ["dotnet-ef", "database", "update", "--project", "/src/FairHire.Infrastructure.Postgres/FairHire.Infrastructure.Postgres.csproj"]
