FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["shared/Hrms.Shared/Hrms.Shared.csproj", "shared/Hrms.Shared/"]
COPY ["backend/services/payroll-report/Hrms.PayrollReport.Api.csproj", "backend/services/payroll-report/"]

RUN dotnet restore "backend/services/payroll-report/Hrms.PayrollReport.Api.csproj"

COPY shared/Hrms.Shared/ shared/Hrms.Shared/
COPY backend/services/payroll-report/ backend/services/payroll-report/

WORKDIR "/src/backend/services/payroll-report"
RUN dotnet publish "Hrms.PayrollReport.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Hrms.PayrollReport.Api.dll"]
