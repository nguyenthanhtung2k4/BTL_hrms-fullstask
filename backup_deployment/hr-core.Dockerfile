FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution-level configurations
COPY Directory.Build.props ./
COPY Directory.Packages.props ./

# Copy shared projects
COPY shared/Hrms.Shared/ shared/Hrms.Shared/
COPY shared/contracts/Hrms.Contracts/ shared/contracts/Hrms.Contracts/

# Copy the hr-core project file
COPY backend/services/hr-core/Hrms.HrCore.Api.csproj backend/services/hr-core/

# Restore dependencies
RUN dotnet restore backend/services/hr-core/Hrms.HrCore.Api.csproj

# Copy the rest of the files for hr-core
COPY backend/services/hr-core/ backend/services/hr-core/

# Build and publish
WORKDIR /src/backend/services/hr-core
RUN dotnet publish Hrms.HrCore.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 5001
ENV ASPNETCORE_URLS=http://+:5001
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Hrms.HrCore.Api.dll"]
