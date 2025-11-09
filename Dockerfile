# --- Build stage ---
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

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "FairHire.API.dll"]