# Multi-Stage Dockerfile for YBT Web Application (.NET 9 + PostgreSQL)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution & project files for layer caching
COPY ["src/Ybt.Core/Ybt.Core.csproj", "src/Ybt.Core/"]
COPY ["src/Ybt.Data/Ybt.Data.csproj", "src/Ybt.Data/"]
COPY ["src/Ybt.Service/Ybt.Service.csproj", "src/Ybt.Service/"]
COPY ["src/Ybt.Web/Ybt.Web.csproj", "src/Ybt.Web/"]

RUN dotnet restore "src/Ybt.Web/Ybt.Web.csproj"

# Copy all source files and publish Release build
COPY . .
WORKDIR "/src/src/Ybt.Web"
RUN dotnet publish "Ybt.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ASP.NET 9 Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Configure default container environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ybt.Web.dll"]
