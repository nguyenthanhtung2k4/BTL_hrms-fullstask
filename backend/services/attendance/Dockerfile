FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["shared/Hrms.Shared/Hrms.Shared.csproj", "shared/Hrms.Shared/"]
COPY ["shared/contracts/Hrms.Contracts/Hrms.Contracts.csproj", "shared/contracts/Hrms.Contracts/"]
COPY ["backend/services/attendance/Hrms.Attendance.Api.csproj", "backend/services/attendance/"]

RUN dotnet restore "backend/services/attendance/Hrms.Attendance.Api.csproj"

COPY shared/Hrms.Shared/ shared/Hrms.Shared/
COPY shared/contracts/Hrms.Contracts/ shared/contracts/Hrms.Contracts/
COPY backend/services/attendance/ backend/services/attendance/

WORKDIR "/src/backend/services/attendance"
RUN dotnet publish "Hrms.Attendance.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Hrms.Attendance.Api.dll"]
