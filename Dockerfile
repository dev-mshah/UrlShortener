# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj
COPY ["TinyUrlService/TinyUrlService.csproj", "TinyUrlService/"]
RUN dotnet restore "TinyUrlService/TinyUrlService.csproj"

# copy everything
COPY . .

# publish
WORKDIR "/src/TinyUrlService"
RUN dotnet publish "TinyUrlService.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TinyUrlService.dll"]