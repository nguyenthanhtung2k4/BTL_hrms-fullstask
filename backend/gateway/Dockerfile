FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["backend/gateway/Hrms.Gateway.csproj", "backend/gateway/"]

RUN dotnet restore "backend/gateway/Hrms.Gateway.csproj"

COPY backend/gateway/ backend/gateway/

WORKDIR "/src/backend/gateway"
RUN dotnet publish "Hrms.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Hrms.Gateway.dll"]
