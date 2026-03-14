# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies (this layer is cached)
COPY ["TinyUrlService.csproj", "./"]
RUN dotnet restore "TinyUrlService.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "TinyUrlService.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Render/Azure often expect traffic on port 8080 or 80
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TinyUrlService.dll"]
