# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
# Browser-reachable API URL (host port mapped to the api service).
ARG API_BASE_URL=http://localhost:5080
WORKDIR /src

COPY ["src/frontend/frontend.csproj", "frontend/"]
RUN dotnet restore "frontend/frontend.csproj"

COPY ["src/frontend/", "frontend/"]
WORKDIR /src/frontend

RUN sed -i "s|https://localhost:7120|${API_BASE_URL}|g" wwwroot/appsettings.json \
    && sed -i "s|https://localhost:7120|${API_BASE_URL}|g" wwwroot/appsettings.Development.json

RUN dotnet publish "frontend.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

FROM nginx:1.27-alpine AS final

COPY ["src/frontend/nginx.conf", "/etc/nginx/nginx.conf"]
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget -qO- http://127.0.0.1:8080/ >/dev/null || exit 1

ENTRYPOINT ["nginx", "-g", "daemon off;"]
