param (
    [Parameter(Mandatory=$true)][string]$file,
    [Parameter(Mandatory=$true)][string]$version,
    [Parameter(Mandatory=$true)][string]$arch,
    [Parameter(Mandatory=$true)][string]$nsis
 )

if ((Test-Path $file) -ne "True") {
    Write-Host "Specified file does not exist. Exiting."
    Exit
}

if ((Test-Path $nsis) -ne "True") {
    Write-Host "Missing NSIS definition. Exiting."
    Exit
}

$pathNSIS = "C:\Program Files (x86)\NSIS\makensis.exe"
$pathSigntool = "C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe"
$pathTemp = Join-Path $env:TEMP (New-Guid).Guid
$pathOrig = Split-Path -Path $file -Parent -Resolve

Write-Host ""
Write-Host "Creating temporary working directory..."
$null = New-Item -ItemType "directory" -Path $pathTemp

Write-Host "Expanding archive to working directory..."
Expand-Archive $file -DestinationPath $pathTemp
$pathProj = Join-Path $pathTemp "Infirmary Integrated"
Copy-Item $nsis -Destination (Join-Path $pathProj "package.nsi")
$tempNSIS = (Join-Path $pathProj "package.nsi")

Write-Host "Updating package details for assembly..."
$find = "`"DisplayName`" `"Infirmary Integrated`" ; <-- Package_Windows.cs EDIT <--"
$replace = "`"DisplayName`" `"Infirmary Integrated $version`""
(Get-Content $tempNSIS).replace($find, $replace) | Set-Content $tempNSIS

$find = "OutFile `"infirmary-integrated-win.exe`""
$replace = "OutFile `"infirmary-integrated-$version-win-$arch.exe`""
$outNSIS = "infirmary-integrated-$version-win-$arch.exe"
(Get-Content $tempNSIS).replace($find, $replace) | Set-Content $tempNSIS

Write-Host "Compiling NSIS package to installation .exe..."
Invoke-Expression "& '$pathNSIS' '$tempNSIS'"

Write-Host "Moving compiled NSIS .exe package to original folder."
$outFile = Join-Path $pathProj $outNSIS
Copy-Item $outFile -Destination $pathOrig
$outFile = Join-Path $pathOrig $outNSIS
Write-Host "Compiled installation executable located at $outFile" -ForegroundColor Green

Write-Host "Removing temporary working directory."
Remove-Item -LiteralPath $pathTemp -Force -Recurse

Write-Host "Signing package using signtool.exe."
Invoke-Expression "& '$pathSigntool' sign /n `"Open Source Developer, Ibi Keller`" /t `"http://time.certum.pl`" /fd SHA256 /v '$outFile'"