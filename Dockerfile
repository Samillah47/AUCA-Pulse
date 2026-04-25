# --- Build stage --------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore .NET dependencies first so this layer can be cached
COPY AUCAPulse.csproj ./
RUN dotnet restore AUCAPulse.csproj

# Bring in the rest of the source
COPY . .

# Restore client-side libraries (Bootstrap, jQuery) via LibMan.
# wwwroot/lib is gitignored, so it must be materialised at build time.
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli && \
    export PATH="$PATH:/root/.dotnet/tools" && \
    libman restore

# Publish a self-contained release build to /app
RUN dotnet publish AUCAPulse.csproj \
        -c Release \
        -o /app \
        --no-restore \
        /p:UseAppHost=false

# --- Runtime stage ------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Render provides PORT; default 8080 for local docker run.
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080

# Bind to whatever port Render hands us via $PORT
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet AUCAPulse.dll"]
