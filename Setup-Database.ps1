[CmdletBinding()]
param(
    [string]$ServerInstance = 'localhost',
    [string]$DatabaseName = 'WezDB'
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSCommandPath
$sqlcmd = Get-Command sqlcmd.exe -ErrorAction Stop

function Invoke-SqlScript {
    param(
        [string]$Database,
        [string]$ScriptPath
    )

    & $sqlcmd.Source -S $ServerInstance -E -C -d $Database -b -i $ScriptPath
    if ($LASTEXITCODE -ne 0) {
        throw "SQL script failed: $ScriptPath"
    }
}

$databaseId = & $sqlcmd.Source -S $ServerInstance -E -C -d master -b -h -1 -W -Q "SET NOCOUNT ON; SELECT DB_ID(N'$($DatabaseName.Replace("'", "''"))');"
if ($LASTEXITCODE -ne 0) {
    throw "Could not connect to SQL Server instance '$ServerInstance' using Windows authentication."
}

if ([string]::IsNullOrWhiteSpace(($databaseId -join ''))) {
    Write-Host "Creating $DatabaseName on $ServerInstance..."
    if ($DatabaseName -ne 'WezDB') {
        throw 'Database.sql creates WezDB. Use the default database name or update Database.sql before changing this parameter.'
    }
    Invoke-SqlScript -Database 'master' -ScriptPath (Join-Path $projectRoot 'Database.sql')
}
else {
    Write-Host "Applying IDENTITY migration to $DatabaseName on $ServerInstance..."
    Invoke-SqlScript -Database $DatabaseName -ScriptPath (Join-Path $projectRoot 'Migrations\AlterExistingTablesToAutoIncrement.sql')
}

Write-Host "Database setup completed successfully." -ForegroundColor Green
