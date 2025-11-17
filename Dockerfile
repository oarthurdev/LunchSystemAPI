# ============================
# 1. Build Stage
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia apenas o csproj primeiro (cache otimizado)
COPY *.sln ./
COPY ./backend/*.csproj ./LunchSystem/
RUN dotnet restore

# Copia o restante do código
COPY . .

# Build da aplicação
RUN dotnet publish -c Release -o /app/publish

# ============================
# 2. Runtime Stage
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copia o publish
COPY --from=build /app/publish .

# Porta padrão do ASP.NET
EXPOSE 8080

# Render usa variável $PORT automaticamente
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Comando de inicialização
ENTRYPOINT ["dotnet", "LunchSystem.dll"]
