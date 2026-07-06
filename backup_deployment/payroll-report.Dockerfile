FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution-level configurations
COPY Directory.Build.props ./
COPY Directory.Packages.props ./

# Copy shared projects
COPY shared/Hrms.Shared/ shared/Hrms.Shared/
COPY shared/contracts/Hrms.Contracts/ shared/contracts/Hrms.Contracts/

# Copy the payroll-report project file
COPY backend/services/payroll-report/Hrms.PayrollReport.Api.csproj backend/services/payroll-report/

# Restore dependencies
RUN dotnet restore backend/services/payroll-report/Hrms.PayrollReport.Api.csproj

# Copy the rest of the files for payroll-report
COPY backend/services/payroll-report/ backend/services/payroll-report/

# Build and publish
WORKDIR /src/backend/services/payroll-report
RUN dotnet publish Hrms.PayrollReport.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 5003
ENV ASPNETCORE_URLS=http://+:5003
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Hrms.PayrollReport.Api.dll"]
