$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$dockerBin = "C:\Program Files\Docker\Docker\resources\bin"

if (Test-Path $dockerBin) {
    $env:PATH = "$dockerBin;$env:PATH"
}

function Test-CommandExists {
    param([Parameter(Mandatory = $true)][string]$Command)
    return [bool](Get-Command $Command -ErrorAction SilentlyContinue)
}

function Start-AppProcess {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string]$ArgumentList,
        [Parameter(Mandatory = $true)][string]$WorkingDirectory
    )

    Write-Host "Starting $Name..."
    Start-Process `
        -FilePath $FilePath `
        -ArgumentList $ArgumentList `
        -WorkingDirectory $WorkingDirectory `
        -WindowStyle Hidden | Out-Null
}

function Test-PortOpen {
    param([Parameter(Mandatory = $true)][int]$Port)
    $connection = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
    return [bool]$connection
}

function Wait-Port {
    param(
        [Parameter(Mandatory = $true)][int]$Port,
        [Parameter(Mandatory = $true)][string]$Name,
        [int]$TimeoutSeconds = 60
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if (Test-PortOpen -Port $Port) {
            Write-Host "$Name is listening on port $Port."
            return
        }
        Start-Sleep -Seconds 2
    }

    throw "$Name did not start listening on port $Port within $TimeoutSeconds seconds."
}

if (-not (Test-CommandExists "dotnet")) {
    throw "dotnet CLI was not found. Install .NET SDK first."
}

if (-not (Test-CommandExists "npm")) {
    throw "npm was not found. Install Node.js first."
}

if (Test-CommandExists "docker") {
    Write-Host "Starting Docker infrastructure..."
    docker compose -f (Join-Path $repoRoot "infra/docker-compose.yml") up -d
}
else {
    Write-Warning "Docker CLI was not found. Skipping SQL Server/RabbitMQ startup."
}

$frontendDir = Join-Path $repoRoot "frontend"
if (-not (Test-Path (Join-Path $frontendDir "node_modules"))) {
    Write-Host "Installing frontend dependencies..."
    Push-Location $frontendDir
    npm ci
    Pop-Location
}

$ports = @(5000, 5001, 5002, 5003, 5173)
foreach ($port in $ports) {
    if (Test-PortOpen -Port $port) {
        Write-Warning "Port $port is already in use. Existing process will be reused."
    }
}

if (-not (Test-PortOpen -Port 5001)) {
    Start-AppProcess `
        -Name "HR Core Service" `
        -FilePath "dotnet" `
        -ArgumentList "run --project backend/services/hr-core/Hrms.HrCore.Api.csproj --launch-profile http" `
        -WorkingDirectory $repoRoot
}

if (-not (Test-PortOpen -Port 5002)) {
    Start-AppProcess `
        -Name "Attendance Service" `
        -FilePath "dotnet" `
        -ArgumentList "run --project backend/services/attendance/Hrms.Attendance.Api.csproj --launch-profile http" `
        -WorkingDirectory $repoRoot
}

if (-not (Test-PortOpen -Port 5003)) {
    Start-AppProcess `
        -Name "Payroll & Report Service" `
        -FilePath "dotnet" `
        -ArgumentList "run --project backend/services/payroll-report/Hrms.PayrollReport.Api.csproj --launch-profile http" `
        -WorkingDirectory $repoRoot
}

if (-not (Test-PortOpen -Port 5000)) {
    Start-AppProcess `
        -Name "API Gateway" `
        -FilePath "dotnet" `
        -ArgumentList "run --project backend/gateway/Hrms.Gateway.csproj --launch-profile http" `
        -WorkingDirectory $repoRoot
}

if (-not (Test-PortOpen -Port 5173)) {
    Start-AppProcess `
        -Name "Frontend" `
        -FilePath "npm.cmd" `
        -ArgumentList "run dev -- --host 127.0.0.1" `
        -WorkingDirectory $frontendDir
}

Wait-Port -Port 5001 -Name "HR Core Service"
Wait-Port -Port 5002 -Name "Attendance Service"
Wait-Port -Port 5003 -Name "Payroll & Report Service"
Wait-Port -Port 5000 -Name "API Gateway"
Wait-Port -Port 5173 -Name "Frontend"

Write-Host ""
Write-Host "HRMS is running."
Write-Host "Frontend: http://localhost:5173"
Write-Host "Gateway HR: http://localhost:5000/api/v1/hr/info"
Write-Host "Gateway Attendance: http://localhost:5000/api/v1/attendance/info"
Write-Host "Gateway Payroll: http://localhost:5000/api/v1/payroll/info"
Write-Host "RabbitMQ UI: http://localhost:15672"

