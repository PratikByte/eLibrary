# eLibrary - Start Backend and Frontend

$RootPath = $PSScriptRoot
$FrontendPath = Join-Path $RootPath "FroentendLovable"

Write-Host "Starting eLibrary..." -ForegroundColor Green

# Start Backend
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$RootPath'; dotnet run"

# Start Frontend
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$FrontendPath'; npm run dev"

Write-Host "Backend started." -ForegroundColor Green
Write-Host "Frontend started." -ForegroundColor Green
Write-Host "eLibrary started successfully!" -ForegroundColor Green