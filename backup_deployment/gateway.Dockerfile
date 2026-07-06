FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution-level configurations
COPY Directory.Build.props ./
COPY Directory.Packages.props ./

# Copy the gateway project file
COPY backend/gateway/Hrms.Gateway.csproj backend/gateway/

# Restore dependencies
RUN dotnet restore backend/gateway/Hrms.Gateway.csproj

# Copy the rest of the files for gateway
COPY backend/gateway/ backend/gateway/

# Build and publish
WORKDIR /src/backend/gateway
RUN dotnet publish Hrms.Gateway.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Hrms.Gateway.dll"]
