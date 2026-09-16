# eLibrary microservices launcher

$ErrorActionPreference = "Stop"
$RootPath = $PSScriptRoot
$FrontendPath = Join-Path $RootPath "FroentendLovable"

$IdentityProject = Join-Path $RootPath "Services\Identity\eLibrary.Identity.API\eLibrary.Identity.API.csproj"
$BookProject = Join-Path $RootPath "Services\Book\eLibrary.Book.API\eLibrary.Book.API.csproj"
$GatewayProject = Join-Path $RootPath "Services\Gateway\eLibrary.Gateway.API\eLibrary.Gateway.API.csproj"

function Assert-PathExists([string]$Path, [string]$Description) {
    if (-not (Test-Path $Path)) {
        throw "$Description was not found: $Path"
    }
}

function Test-TcpPort([string]$HostName, [int]$Port) {
    try {
        return (Test-NetConnection -ComputerName $HostName -Port $Port -InformationLevel Quiet)
    }
    catch {
        return $false
    }
}

function Start-DotnetService([string]$Name, [string]$ProjectPath) {
    $serviceDirectory = Split-Path $ProjectPath -Parent

    Start-Process powershell -ArgumentList @(
        "-NoExit",
        "-Command",
        "`$Host.UI.RawUI.WindowTitle = '$Name'; Set-Location '$serviceDirectory'; dotnet run --launch-profile http --project '$ProjectPath'"
    )

    Write-Host "$Name started." -ForegroundColor Green
}

Assert-PathExists $IdentityProject "Identity project"
Assert-PathExists $BookProject "Book project"
Assert-PathExists $GatewayProject "Gateway project"
Assert-PathExists $FrontendPath "Frontend directory"

if (-not (Test-TcpPort "localhost" 5432)) {
    Write-Warning "PostgreSQL is not reachable on localhost:5432. Start PostgreSQL before using the APIs."
}

if (-not (Test-TcpPort "localhost" 27017)) {
    Write-Warning "MongoDB is not reachable on localhost:27017. Start MongoDB before using media features."
}

Write-Host "Starting eLibrary microservices..." -ForegroundColor Cyan

Start-DotnetService "IDENTITY API - 8001" $IdentityProject
Start-DotnetService "BOOK API - 8002" $BookProject
Start-DotnetService "GATEWAY API - 8000" $GatewayProject

Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "`$Host.UI.RawUI.WindowTitle = 'FRONTEND - 3001'; Set-Location '$FrontendPath'; npm run dev"
)

Write-Host "Frontend started on port 3001." -ForegroundColor Green

Write-Host ""
Write-Host "eLibrary services are launching:" -ForegroundColor Cyan
Write-Host "  Gateway:  http://localhost:8000"
Write-Host "  Identity: http://localhost:8001"
Write-Host "  Book:     http://localhost:8002"
Write-Host "  Frontend: http://localhost:3001"