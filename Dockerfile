# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Copy csproj and restore dependencies
COPY backend/WoWGame.Api/WoWGame.Api.csproj ./backend/WoWGame.Api/
RUN dotnet restore ./backend/WoWGame.Api/WoWGame.Api.csproj

# Copy everything else and publish release
COPY backend/WoWGame.Api/ ./backend/WoWGame.Api/
RUN dotnet publish ./backend/WoWGame.Api/WoWGame.Api.csproj -c Release -o out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose port 5194
EXPOSE 5194
ENV ASPNETCORE_URLS=http://+:5194

# Run the app
ENTRYPOINT ["dotnet", "WoWGame.Api.dll"]
