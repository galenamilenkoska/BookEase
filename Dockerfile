# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["BookEase.csproj", "."]
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render injects $PORT at runtime (usually 10000).
# Shell-form CMD lets the variable expand correctly.
# Default to 8080 so plain `docker run` still works locally.
CMD ASPNETCORE_URLS="http://+:${PORT:-8080}" dotnet BookEase.dll
