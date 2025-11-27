# Backend Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files for restore
COPY src/RapidPhotoFlow.Domain/RapidPhotoFlow.Domain.csproj src/RapidPhotoFlow.Domain/
COPY src/RapidPhotoFlow.Application/RapidPhotoFlow.Application.csproj src/RapidPhotoFlow.Application/
COPY src/RapidPhotoFlow.Infrastructure/RapidPhotoFlow.Infrastructure.csproj src/RapidPhotoFlow.Infrastructure/
COPY src/RapidPhotoFlow.Api/RapidPhotoFlow.Api.csproj src/RapidPhotoFlow.Api/

# Restore dependencies (only for API and its dependencies, not tests)
RUN dotnet restore src/RapidPhotoFlow.Api/RapidPhotoFlow.Api.csproj

# Copy all source code
COPY src/ src/

# Build and publish
WORKDIR /src/src/RapidPhotoFlow.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create uploads directory
RUN mkdir -p /app/uploads /app/logs

# Copy published app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:7001
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 7001

ENTRYPOINT ["dotnet", "RapidPhotoFlow.Api.dll"]
