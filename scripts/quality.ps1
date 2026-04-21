[CmdletBinding()]
param(
    [ValidateSet("all", "build", "test", "report", "format")]
    [string] $Task = "all",

    [string] $Configuration = "Release",

    [switch] $SkipBuild,

    [switch] $NoProgress
)

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$Solution = Join-Path $RepoRoot "Source/NexusForever.sln"
$QualityProject = Join-Path $RepoRoot "Source/NexusForever.CodeQuality/NexusForever.CodeQuality.csproj"
$ReportRoot = Join-Path $RepoRoot "artifacts/code-quality/latest"
$TestResultsRoot = Join-Path $RepoRoot "artifacts/TestResults"
$BuildLog = Join-Path $ReportRoot "build.log"

New-Item -ItemType Directory -Force -Path $ReportRoot | Out-Null
New-Item -ItemType Directory -Force -Path $TestResultsRoot | Out-Null

function Invoke-LoggedBuild {
    Write-Host "Building $Solution ($Configuration)..."
    dotnet build $Solution -c $Configuration --nologo 2>&1 | Tee-Object -FilePath $BuildLog
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE"
    }
}

function Get-TestProjects {
    Get-ChildItem -Path (Join-Path $RepoRoot "Source") -Filter "*.csproj" -Recurse |
        Where-Object {
            $_.BaseName -match "Tests?$" -or
            (Select-String -Path $_.FullName -Pattern "<IsTestProject>\s*true\s*</IsTestProject>" -Quiet)
        }
}

function Invoke-Tests {
    $testProjects = @(Get-TestProjects)
    if ($testProjects.Count -eq 0) {
        Write-Host "No test projects detected; skipping dotnet test and marking coverage unavailable in the report."
        return
    }

    Write-Host "Running tests with coverage collection..."
    dotnet test $Solution `
        -c $Configuration `
        --no-build `
        --results-directory $TestResultsRoot `
        --collect:"XPlat Code Coverage" `
        --settings (Join-Path $RepoRoot "eng/CodeCoverage.runsettings")

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed with exit code $LASTEXITCODE"
    }
}

function Invoke-QualityReport {
    Write-Host "Generating C# quality report..."
    dotnet run --project $QualityProject -c $Configuration -- `
        --source-root (Join-Path $RepoRoot "Source") `
        --output $ReportRoot `
        --build-log $BuildLog `
        --coverage-root $TestResultsRoot

    if ($LASTEXITCODE -ne 0) {
        throw "quality report generation failed with exit code $LASTEXITCODE"
    }
}

function Invoke-FormatCheck {
    Write-Host "Checking dotnet formatting without changing files..."
    dotnet format $Solution --verify-no-changes --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet format found changes to apply"
    }
}

switch ($Task) {
    "build" {
        Invoke-LoggedBuild
    }
    "test" {
        if (-not $SkipBuild) {
            Invoke-LoggedBuild
        }
        Invoke-Tests
    }
    "report" {
        Invoke-QualityReport
    }
    "format" {
        Invoke-FormatCheck
    }
    "all" {
        if (-not $SkipBuild) {
            Invoke-LoggedBuild
        }
        Invoke-Tests
        Invoke-QualityReport
    }
}

Write-Host "Quality task '$Task' complete. Report root: $ReportRoot"
