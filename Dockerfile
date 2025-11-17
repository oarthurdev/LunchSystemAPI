# ============================
# 1. Build Stage
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia somente a solução e csproj para usar cache do Docker
COPY lunch-choice.sln ./
COPY LunchSystem.csproj ./

# Restaura dependências
RUN dotnet restore

# Copia o restante do código
COPY . .

# Publica a aplicação
RUN dotnet publish -c Release -o /app/publish

# ============================
# 2. Runtime Stage
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copia o publish da imagem anterior
COPY --from=build /app/publish .

# Render usa automaticamente a variável PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Inicia a aplicação
ENTRYPOINT ["dotnet", "LunchSystem.dll"]
