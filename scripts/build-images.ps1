param(
    [ValidateSet("identity", "book", "gateway", "frontend")]
    [string[]]$Services = @("identity", "book", "gateway", "frontend")
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path $PSScriptRoot -Parent
$envFile = Join-Path $repositoryRoot ".env"
$logDirectory = Join-Path $repositoryRoot "logs"
$logFile = Join-Path $logDirectory ("docker-image-build-{0}.log" -f (Get-Date -Format "yyyyMMdd-HHmmss"))
$composeArguments = @(
    "compose",
    "--env-file", ".env",
    "-f", "compose.yaml",
    "-f", "compose.build.yaml"
)

function Invoke-LoggedDockerCommand {
    param(
        [string[]]$Arguments,
        [string]$LogPath
    )

    $commandLine = "docker " + ($Arguments -join " ")
    Write-Output "`n> $commandLine" | Tee-Object -FilePath $LogPath -Append
    & docker @Arguments 2>&1 | Tee-Object -FilePath $LogPath -Append
    if ($LASTEXITCODE -ne 0) {
        throw "Docker command failed with exit code $LASTEXITCODE"
    }
}

Push-Location $repositoryRoot
try {
    New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
    "eLibrary Docker image build started: $(Get-Date -Format o)" |
        Set-Content -LiteralPath $logFile -Encoding utf8

    if (-not (Test-Path -LiteralPath $envFile)) {
        throw "Missing .env. Copy .env.example to .env and set private values first."
    }

    $engineType = & docker info --format '{{.OSType}}' 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Docker Engine is unavailable: $engineType"
    }
    if ($engineType -ne "linux") {
        throw "Docker Desktop must be running Linux containers; detected engine type '$engineType'."
    }

    Invoke-LoggedDockerCommand `
        -Arguments ($composeArguments + @("config", "--quiet")) `
        -LogPath $logFile

    foreach ($service in $Services) {
        Write-Output "`nBuilding $service image..." | Tee-Object -FilePath $logFile -Append
        Invoke-LoggedDockerCommand `
            -Arguments ($composeArguments + @("build", "--pull", $service)) `
            -LogPath $logFile
    }

    "Requested application images built successfully: $($Services -join ', '): $(Get-Date -Format o)" |
        Tee-Object -FilePath $logFile -Append
    Write-Output "`nBuild complete. Full output saved to: $logFile"
}
catch {
    "BUILD FAILED: $($_.Exception.Message)" | Tee-Object -FilePath $logFile -Append
    Write-Error "Image build failed. Review the log: $logFile"
    exit 1
}
finally {
    Pop-Location
}
