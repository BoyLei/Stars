param(
    [string]$LogPath = "Temp/NetworkRefactorV2-EditMode.log",
    [string]$ResultsPath = "Temp/NetworkRefactorV2-EditMode-results.xml"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $LogPath)) {
    Write-Host "STATUS: INCONCLUSIVE"
    Write-Host "Log not found: $LogPath"
    exit 2
}

$lines = Get-Content -LiteralPath $LogPath
$text = $lines -join "`n"

$compileErrors = $lines | Select-String -Pattern '\berror CS\d+|Scripts have compiler errors'
$testFailures = $lines | Select-String -Pattern '(^|\b)(FAIL\b|Failed:\s*[1-9]\d*|failed tests?:\s*[1-9]\d*|AssertionException|Unhandled Exception|Test failed|Test\(s\) failed)'
$licenseIssues = $lines | Select-String -Pattern 'Licensing.*Error|Access token is unavailable|failed validation'
$testSummary = $lines | Select-String -Pattern 'Test run finished|Run finished|Passed:\s*\d+|Failed:\s*\d+|test results saved|Test results written' | Select-Object -Last 20
$v2Mentions = $lines | Select-String -Pattern 'NetworkRefactorV2' | Select-Object -Last 20
$resultsMentioned = $text.Contains("-testResults") -or $text.Contains($ResultsPath.Replace("\", "/")) -or $text.Contains($ResultsPath)
$resultsExists = Test-Path -LiteralPath $ResultsPath
$xmlSummary = $null
if ($resultsExists) {
    try {
        [xml]$xml = Get-Content -LiteralPath $ResultsPath
        $xmlSummary = $xml.'test-run'
    } catch {
        $testFailures += "Cannot parse results XML: $($_.Exception.Message)"
    }
}

$hasCompileError = $compileErrors.Count -gt 0
$hasFailure = $testFailures.Count -gt 0
$hasSummary = $testSummary.Count -gt 0 -or $xmlSummary -ne $null

if ($hasCompileError -or $hasFailure) {
    Write-Host "STATUS: FAILED"
    $exitCode = 1
} elseif ($hasSummary) {
    Write-Host "STATUS: OK_OR_CHECK_SUMMARY"
    $exitCode = 0
} else {
    Write-Host "STATUS: INCONCLUSIVE"
    $exitCode = 2
}

Write-Host "Log: $LogPath"
Write-Host "Results: $ResultsPath"
Write-Host "Lines: $($lines.Count)"
Write-Host "CompileErrors: $($compileErrors.Count)"
Write-Host "TestFailureHints: $($testFailures.Count)"
Write-Host "TestSummaryHints: $($testSummary.Count)"
Write-Host "LicenseIssueHints: $($licenseIssues.Count)"
Write-Host "ResultsMentioned: $resultsMentioned"
Write-Host "ResultsExists: $resultsExists"

if (-not $hasSummary) {
    Write-Host "Note: no Unity test summary/result line was found; this log cannot prove EditMode tests passed."
}
if ($resultsMentioned -and -not $resultsExists) {
    Write-Host "Problem: -testResults was requested, but the XML result file was not created."
}

if ($compileErrors.Count -gt 0) {
    Write-Host ""
    Write-Host "== Compile errors =="
    $compileErrors | Select-Object -First 40 | ForEach-Object { Write-Host $_.Line }
}

if ($testFailures.Count -gt 0) {
    Write-Host ""
    Write-Host "== Test failure hints =="
    $testFailures | Select-Object -First 40 | ForEach-Object { Write-Host $_.Line }
}

if ($testSummary.Count -gt 0) {
    Write-Host ""
    Write-Host "== Test summary hints =="
    $testSummary | ForEach-Object { Write-Host $_.Line }
}

if ($xmlSummary -ne $null) {
    Write-Host ""
    Write-Host "== XML result summary =="
    Write-Host "result=$($xmlSummary.result) total=$($xmlSummary.total) passed=$($xmlSummary.passed) failed=$($xmlSummary.failed) inconclusive=$($xmlSummary.inconclusive) skipped=$($xmlSummary.skipped) duration=$($xmlSummary.duration)"
}

if ($licenseIssues.Count -gt 0) {
    Write-Host ""
    Write-Host "== License issue hints =="
    $licenseIssues | Select-Object -Last 20 | ForEach-Object { Write-Host $_.Line }
}

if ($v2Mentions.Count -gt 0) {
    Write-Host ""
    Write-Host "== Last NetworkRefactorV2 lines =="
    $v2Mentions | ForEach-Object { Write-Host $_.Line }
}

exit $exitCode
