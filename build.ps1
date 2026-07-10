$ErrorActionPreference = 'Stop'

$env:DOTNET_CLI_HOME = Join-Path $PSScriptRoot '.dotnet'
$env:NUGET_PACKAGES = Join-Path $PSScriptRoot '.nuget\packages'

$projectPath = Join-Path $PSScriptRoot 'Assembly-CSharp.csproj'

dotnet build $projectPath
