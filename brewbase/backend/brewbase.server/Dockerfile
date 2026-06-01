# BrewBase API — production image (Render / Docker)
# Build context: this directory (brewbase/backend/brewbase.server)

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY brewbase.server.csproj .
RUN dotnet restore brewbase.server.csproj

COPY . .
RUN dotnet publish brewbase.server.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

# ICU for correct string/culture handling; non-root user for runtime
RUN apk add --no-cache icu-libs \
    && addgroup -S appgroup \
    && adduser -S appuser -G appgroup

COPY --from=build /app/publish .
COPY docker-entrypoint.sh .
RUN chmod +x docker-entrypoint.sh \
    && chown -R appuser:appgroup /app

USER appuser

EXPOSE 8080

ENTRYPOINT ["./docker-entrypoint.sh"]
