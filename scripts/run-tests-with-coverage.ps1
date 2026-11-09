# Запускает тесты и сохраняет HTML-отчёт о покрытии в tests/coverage-report.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectDirectory = Split-Path -Path $PSScriptRoot -Parent
Push-Location -LiteralPath $projectDirectory
try {
    dotnet tool restore --tool-manifest dotnet-tools.json
    if ($LASTEXITCODE -ne 0) {
        throw 'Не удалось восстановить инструменты отчёта.'
    }

    $resultsDirectory = Join-Path 'tests/coverage' ([Guid]::NewGuid().ToString('N'))
    dotnet test --settings tests/tests.runsettings --collect 'XPlat Code Coverage' --results-directory $resultsDirectory
    if ($LASTEXITCODE -ne 0) {
        throw 'Тесты завершились с ошибкой.'
    }

    dotnet tool run reportgenerator "-reports:$resultsDirectory/**/coverage.cobertura.xml" '-targetdir:tests/coverage-report' '-reporttypes:Html;Cobertura;JsonSummary'
    if ($LASTEXITCODE -ne 0) {
        throw 'Не удалось создать отчёт о покрытии.'
    }
}
finally {
    Pop-Location
}
