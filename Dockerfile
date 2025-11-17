# ============================
# 1. Build Stage
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore lunch-choice.sln

RUN dotnet publish LunchSystem.csproj -c Release -o /app/publish

# ============================
# 2. Runtime Stage
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copia TUDO que foi publicado
COPY --from=build /app/publish .

# Copia explicitamente o appsettings.json
COPY --from=build appsettings.json /app/appsettings.json

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "LunchSystem.dll"]
