# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers for optimization
COPY API_v2.csproj ./
RUN dotnet restore

# Copy everything else and publish the project
COPY . ./
RUN dotnet publish API_v2.csproj -c Release -o /app --no-restore

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

# Expose default port (Render will use PORT environment variable at runtime)
EXPOSE 8080

# Configure port dynamically using Render's PORT environment variable and run the application
# Using 'exec' ensures OS signals (like SIGTERM) are forwarded properly to the dotnet process
CMD ["sh", "-c", "export ASPNETCORE_HTTP_PORTS=${PORT:-8080} && exec dotnet API_v2.dll"]