param([switch]$RunIntegration)
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) { $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
if (-not (Test-Path $csc)) { throw '.NET Framework C# compiler not found. Windows with .NET Framework 4.8 is required.' }
New-Item -ItemType Directory -Path 'build','build\tests' -Force | Out-Null
function Compile([string[]]$CompilerArgs) { & $csc /nologo /codepage:65001 /optimize+ @CompilerArgs; if ($LASTEXITCODE -ne 0) { throw 'C# compilation failed.' } }
function RunTest([string]$Exe, [string[]]$TestArgs = @()) { $options = @{FilePath=(Resolve-Path $Exe).Path; Wait=$true; PassThru=$true; NoNewWindow=$true}; if ($TestArgs.Count -gt 0) { $options.ArgumentList=$TestArgs }; $process=Start-Process @options; if ($process.ExitCode -ne 0) { throw "Test failed: $Exe (exit $($process.ExitCode))" } }
Compile @('/target:winexe','/out:build\PCZamanlayici.exe','/win32icon:assets\icon.ico','/r:System.Windows.Forms.dll','/r:System.Drawing.dll','/r:System.Xml.Linq.dll','/r:Microsoft.CSharp.dll','src\App.cs','src\Core.cs','src\Recurring.cs','src\DailyScheduler.cs','src\AssemblyInfo.cs')
Compile @('/out:build\tests\ExtendedTests.exe','src\Core.cs','tests\ExtendedTests.cs')
Compile @('/out:build\tests\DailyTests.exe','/r:System.Xml.Linq.dll','src\Core.cs','src\Recurring.cs','tests\DailyTests.cs')
RunTest '.\build\tests\ExtendedTests.exe'
RunTest '.\build\tests\DailyTests.exe'
Compile @('/main:SmokeTests','/out:build\tests\SmokeTests.exe','/r:System.Windows.Forms.dll','/r:System.Drawing.dll','/r:System.Xml.Linq.dll','/r:Microsoft.CSharp.dll','src\App.cs','src\Core.cs','src\Recurring.cs','src\DailyScheduler.cs','tests\SmokeTests.cs')
RunTest '.\build\tests\SmokeTests.exe'
$smokeOutput = Join-Path $PSScriptRoot ('build\tests\smoke-' + [Guid]::NewGuid().ToString('N'))
RunTest '.\build\PCZamanlayici.exe' @('--smoke', ('--smoke-output="' + $smokeOutput + '"'))
$report = Join-Path $smokeOutput 'smoke-result.txt'
if (-not (Test-Path $report) -or -not ((Get-Content $report -Raw).StartsWith('PASS:'))) { throw 'UI smoke report failed.' }
New-Item -ItemType Directory -Path 'docs' -Force | Out-Null
Copy-Item (Join-Path $smokeOutput 'ui-preview-v2.png') 'docs\screenshot.png' -Force
Remove-Item $smokeOutput -Recurse -Force
if ($RunIntegration) {
 Compile @('/target:winexe','/out:build\tests\SchedulerNoop.exe','tests\SchedulerNoop.cs')
 Compile @('/out:build\tests\DailyIntegrationTest.exe','/r:System.Xml.Linq.dll','/r:Microsoft.CSharp.dll','src\Core.cs','src\Recurring.cs','src\DailyScheduler.cs','tests\DailyIntegrationTest.cs')
 RunTest '.\build\tests\DailyIntegrationTest.exe'
}
Write-Output 'BUILD AND SAFE TESTS PASSED. No real power operation was executed.'
