# ============================
# 1. Build Stage
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY lunch-choice.sln ./
COPY LunchSystem.csproj ./

RUN dotnet restore lunch-choice.sln

COPY . .

# Publica com arquivo explicitado para evitar erros
RUN dotnet publish LunchSystem.csproj -c Release -o /app/publish

# ============================
# 2. Runtime Stage
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "LunchSystem.dll"]
