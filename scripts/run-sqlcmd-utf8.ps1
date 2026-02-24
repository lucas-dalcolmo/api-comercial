param(
    [Parameter(Mandatory = $true)]
    [string]$SqlFilePath,
    [string]$Server = "LUCAS_DALCOLMO,1433",
    [string]$Database = "Apeiron_ONE",
    [string]$User = "ApeironOne_Service",
    [string]$Password = "apeironOne_Service#831"
)

if (-not (Test-Path -LiteralPath $SqlFilePath)) {
    throw "SQL file not found: $SqlFilePath"
}

$fullPath = (Resolve-Path -LiteralPath $SqlFilePath).Path

Write-Host "Running script with UTF-8 input/output:" $fullPath
sqlcmd `
    -f 65001 `
    -S $Server `
    -d $Database `
    -U $User `
    -P $Password `
    -i $fullPath

if ($LASTEXITCODE -ne 0) {
    throw "sqlcmd failed with exit code $LASTEXITCODE"
}

Write-Host "Done."
