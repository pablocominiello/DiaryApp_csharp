# ========================================
# Etapa 1: Build
# ========================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto en orden de dependencias
COPY ["DiaryApp.Shared/DiaryApp.Shared.csproj", "DiaryApp.Shared/"]
COPY ["DiaryApp.Core/DiaryApp.Core.csproj", "DiaryApp.Core/"]
COPY ["DiaryApp/DiaryApp.csproj", "DiaryApp/"]

# Restaurar dependencias
RUN dotnet restore "DiaryApp/DiaryApp.csproj"

# Copiar el resto del código
COPY DiaryApp.Shared/ DiaryApp.Shared/
COPY DiaryApp.Core/ DiaryApp.Core/
COPY DiaryApp/ DiaryApp/

# Compilar y publicar
WORKDIR "/src/DiaryApp"
RUN dotnet publish "DiaryApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ========================================
# Etapa 2: Runtime
# ========================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Crear usuario no-root para seguridad
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copiar los binarios desde la etapa de build
COPY --from=build /app/publish .

# Exponer puerto
EXPOSE 8080

# Variables de entorno por defecto
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "DiaryApp.dll"]