FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["shared/Hrms.Shared/Hrms.Shared.csproj", "shared/Hrms.Shared/"]
COPY ["shared/contracts/Hrms.Contracts/Hrms.Contracts.csproj", "shared/contracts/Hrms.Contracts/"]
COPY ["backend/services/hr-core/Hrms.HrCore.Api.csproj", "backend/services/hr-core/"]

RUN dotnet restore "backend/services/hr-core/Hrms.HrCore.Api.csproj"

COPY shared/Hrms.Shared/ shared/Hrms.Shared/
COPY shared/contracts/Hrms.Contracts/ shared/contracts/Hrms.Contracts/
COPY backend/services/hr-core/ backend/services/hr-core/

WORKDIR "/src/backend/services/hr-core"
RUN dotnet publish "Hrms.HrCore.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Hrms.HrCore.Api.dll"]
