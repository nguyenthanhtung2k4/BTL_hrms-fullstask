$ErrorActionPreference = "Stop"

$ports = @(5000, 5001, 5002, 5003, 5173)
$processIds = foreach ($port in $ports) {
    Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty OwningProcess
}

$uniqueProcessIds = $processIds | Sort-Object -Unique

if (-not $uniqueProcessIds) {
    Write-Host "No app processes found on ports: $($ports -join ', ')."
    return
}

foreach ($processId in $uniqueProcessIds) {
    try {
        $process = Get-Process -Id $processId -ErrorAction Stop
        Write-Host "Stopping $($process.ProcessName) ($processId)..."
        Stop-Process -Id $processId -Force
    }
    catch {
        Write-Warning "Could not stop process ${processId}: $($_.Exception.Message)"
    }
}

Write-Host "Stopped app processes on ports: $($ports -join ', ')."

