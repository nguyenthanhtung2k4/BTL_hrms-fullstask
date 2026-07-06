FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution-level configurations
COPY Directory.Build.props ./
COPY Directory.Packages.props ./

# Copy shared projects
COPY shared/Hrms.Shared/ shared/Hrms.Shared/
COPY shared/contracts/Hrms.Contracts/ shared/contracts/Hrms.Contracts/

# Copy the attendance project file
COPY backend/services/attendance/Hrms.Attendance.Api.csproj backend/services/attendance/

# Restore dependencies
RUN dotnet restore backend/services/attendance/Hrms.Attendance.Api.csproj

# Copy the rest of the files for attendance
COPY backend/services/attendance/ backend/services/attendance/

# Build and publish
WORKDIR /src/backend/services/attendance
RUN dotnet publish Hrms.Attendance.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 5002
ENV ASPNETCORE_URLS=http://+:5002
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Hrms.Attendance.Api.dll"]
