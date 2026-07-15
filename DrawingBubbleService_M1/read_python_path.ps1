param([Parameter(Mandatory=$true)][string]$AppSettingsPath)

$content = Get-Content -Raw $AppSettingsPath
if ($content -match '"PythonPath"\s*:\s*\{\s*"Path"\s*:\s*"([^"]+)"') {
    $matches[1] -replace '\\\\','\'
}
