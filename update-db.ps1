[CmdletBinding()]
param(
    [string] $DotnetEf,

    [string] $Configuration,

    [switch] $NoBuild,

    [switch] $CopyExampleConfigs,

    [switch] $SkipConfigCheck,

    [switch] $DryRun,

    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]] $EfArgs
)

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSCommandPath
$SourceDir = Join-Path $RepoRoot "Source"

$ConfigFiles = @(
    @{
        Project = "NexusForever.WorldServer"
        Config = "WorldServer.json"
        Example = "WorldServer.example.json"
    },
    @{
        Project = "NexusForever.Server.ChatServer"
        Config = "ChatServer.json"
        Example = "ChatServer.example.json"
    },
    @{
        Project = "NexusForever.Server.GroupServer"
        Config = "GroupServer.json"
        Example = "GroupServer.example.json"
    }
)

function Split-CommandLine {
    param(
        [Parameter(Mandatory)]
        [string] $CommandLine
    )

    $parts = @()
    $matches = [regex]::Matches($CommandLine, '"(?:[^"]|"")*"|''(?:[^'']|'''')*''|\S+')

    foreach ($match in $matches) {
        $value = $match.Value
        if (($value.StartsWith('"') -and $value.EndsWith('"')) -or ($value.StartsWith("'") -and $value.EndsWith("'"))) {
            $value = $value.Substring(1, $value.Length - 2)
        }

        $parts += $value
    }

    return $parts
}

function Resolve-EfCommand {
    if ($DotnetEf) {
        return @(Split-CommandLine -CommandLine $DotnetEf)
    }

    if ($env:DOTNET_EF) {
        return @(Split-CommandLine -CommandLine $env:DOTNET_EF)
    }

    $globalTool = Join-Path $HOME ".dotnet\tools\dotnet-ef.exe"
    if (Test-Path $globalTool) {
        return @($globalTool)
    }

    $dotnetEf = Get-Command dotnet-ef -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($dotnetEf) {
        return @($dotnetEf.Source)
    }

    return @("dotnet", "ef")
}

function Get-ConfigState {
    $state = @()

    foreach ($entry in $ConfigFiles) {
        $projectDir = Join-Path $SourceDir $entry.Project
        $configPath = Join-Path $projectDir $entry.Config
        $examplePath = Join-Path $projectDir $entry.Example

        $state += [PSCustomObject]@{
            Project = $entry.Project
            ConfigPath = $configPath
            ExamplePath = $examplePath
            Exists = Test-Path $configPath
            ExampleExists = Test-Path $examplePath
        }
    }

    return $state
}

function Ensure-ConfigFiles {
    param(
        [switch] $CopyMissing
    )

    $state = @(Get-ConfigState)
    $missing = @($state | Where-Object { -not $_.Exists })

    if ($missing.Count -eq 0 -or $SkipConfigCheck) {
        return
    }

    if ($CopyMissing) {
        foreach ($entry in $missing) {
            if (-not $entry.ExampleExists) {
                throw "Missing example config '$($entry.ExamplePath)' for project '$($entry.Project)'."
            }

            if ($DryRun) {
                Write-Host "DRY RUN: copy $($entry.ExamplePath) -> $($entry.ConfigPath)"
                continue
            }

            Copy-Item -Path $entry.ExamplePath -Destination $entry.ConfigPath
            Write-Host "Copied $($entry.ExamplePath) -> $($entry.ConfigPath)"
        }

        return
    }

    $details = $missing | ForEach-Object {
        "- $($_.ConfigPath) (copy from $($_.ExamplePath))"
    }

    throw @"
Missing config file(s) required by the EF design-time factories:
$($details -join [Environment]::NewLine)

Run .\update-db.ps1 -CopyExampleConfigs to create the missing files, or copy
them manually as described in the Windows installation guide.
"@
}

function Invoke-EfCommand {
    param(
        [Parameter(Mandatory)]
        [string] $WorkingDirectory,

        [Parameter(Mandatory)]
        [string[]] $Arguments
    )

    $command = $script:EfCommand[0]
    $commandArgs = @()
    if ($script:EfCommand.Count -gt 1) {
        $commandArgs = $script:EfCommand[1..($script:EfCommand.Count - 1)]
    }

    if ($DryRun) {
        $display = @($command, $commandArgs, $Arguments) -join " "
        Write-Host "DRY RUN: cd $WorkingDirectory"
        Write-Host "DRY RUN: $display"
        return
    }

    Push-Location $WorkingDirectory
    try {
        & $command @commandArgs @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "EF command failed with exit code $LASTEXITCODE"
        }
    }
    finally {
        Pop-Location
    }
}

function Invoke-DatabaseUpdate {
    param(
        [Parameter(Mandatory)]
        [string] $HostProject,

        [Parameter(Mandatory)]
        [string] $DatabaseName,

        [Parameter(Mandatory)]
        [string[]] $Arguments
    )

    Write-Host "==> Updating $DatabaseName database"
    $projectDir = Join-Path $SourceDir $HostProject
    Invoke-EfCommand -WorkingDirectory $projectDir -Arguments $Arguments
}

$EfCommand = @(Resolve-EfCommand)
$CommonEfArgs = @()

if ($Configuration) {
    $CommonEfArgs += @("--configuration", $Configuration)
}

if ($NoBuild) {
    $CommonEfArgs += "--no-build"
}

if ($EfArgs) {
    $CommonEfArgs += $EfArgs
}

Ensure-ConfigFiles -CopyMissing:$CopyExampleConfigs

Invoke-DatabaseUpdate "NexusForever.WorldServer" "Auth" @("database", "update", "--context", "AuthContext") + $CommonEfArgs
Invoke-DatabaseUpdate "NexusForever.WorldServer" "Character" @("database", "update", "--context", "CharacterContext") + $CommonEfArgs
Invoke-DatabaseUpdate "NexusForever.WorldServer" "World" @("database", "update", "--context", "WorldContext") + $CommonEfArgs
Invoke-DatabaseUpdate "NexusForever.Server.ChatServer" "Chat" @("database", "update") + $CommonEfArgs
Invoke-DatabaseUpdate "NexusForever.Server.GroupServer" "Group" @("database", "update") + $CommonEfArgs

Write-Host "==> Database updates complete"
