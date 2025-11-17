# ============================
# 1. Build Stage
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia a solução e o projeto
COPY lunch-choice.sln ./
COPY LunchSystem.csproj ./

# Restaura dependências (usando o .sln para evitar conflitos)
RUN dotnet restore lunch-choice.sln

# Copia o restante do código
COPY . .

# Publica a aplicação
RUN dotnet publish -c Release -o /app/publish

# ============================
# 2. Runtime Stage
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copia a build gerada
COPY --from=build /app/publish .

# Porta padrão do Render
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Start
ENTRYPOINT ["dotnet", "LunchSystem.dll"]
