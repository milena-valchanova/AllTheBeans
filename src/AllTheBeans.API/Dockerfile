# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["src/AllTheBeans.API/AllTheBeans.API.csproj", "src/AllTheBeans.API/"]
COPY ["src/AllTheBeans.Infrastructure/AllTheBeans.Infrastructure.csproj", "src/AllTheBeans.Infrastructure/"]
COPY ["src/AllTheBeans.Domain/AllTheBeans.Domain.csproj", "src/AllTheBeans.Domain/"]
RUN dotnet restore "./src/AllTheBeans.API/AllTheBeans.API.csproj"
COPY . .
WORKDIR "/src/src/AllTheBeans.API"
RUN dotnet build "./AllTheBeans.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AllTheBeans.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

USER $APP_UID

ENTRYPOINT ["dotnet", "AllTheBeans.API.dll"]