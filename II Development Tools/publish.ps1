param (
    [switch]$signonly = $false
 )

# Definitions
$pathNSIS = "C:\Program Files (x86)\NSIS\makensis.exe"
$pathSigntool = "C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe"

$defNSIS = Resolve-Path "..\Package, Windows\package-win.nsi"
$pathRelease = Resolve-Path "..\Release\"

Write-Host ""

# Process each .zip file in Release with -win- flag in filename
if (-Not $signonly) {
    foreach ($file in Get-ChildItem -Path $pathRelease -Include *-win-*.zip -Name) {
        $file = Join-Path $pathRelease $file
        $l = Split-Path $file -Leaf
        $lb = (Get-Item $file).BaseName
        
        $s = $lb.Substring("infirmary-integrated-".Length)
        $version = $s.Substring(0, $s.IndexOf('-'))
        $arch = $s.Substring($s.LastIndexOf('-') + 1)

        Write-Host "Processing package $l" -ForegroundColor Blue
        Write-Host "  Version: $version" -ForegroundColor Blue
        Write-Host "  Architecture: $arch" -ForegroundColor Blue

        # Determine working directory
        $pathTemp = Join-Path $env:TEMP (New-Guid).Guid
        $pathOrig = Split-Path -Path $file -Parent -Resolve

        # Create working directory
        Write-Host ">> Creating temporary working directory..."
        $null = New-Item -ItemType "directory" -Path $pathTemp

        # Expand .zip package to working directory for re-packaging
        Write-Host ">> Expanding archive to working directory..."
        Expand-Archive $file -DestinationPath $pathTemp
        $pathProj = Join-Path $pathTemp "Infirmary Integrated"
        Copy-Item $defNSIS -Destination (Join-Path $pathProj "package.nsi")
        $tempNSIS = (Join-Path $pathProj "package.nsi")

        # Modify NSIS configuration flags
        Write-Host ">> Updating package details for assembly..."
        $find = "`"DisplayName`" `"Infirmary Integrated`""
        $replace = "`"DisplayName`" `"Infirmary Integrated $version`""
        (Get-Content $tempNSIS).replace($find, $replace) | Set-Content $tempNSIS

        $find = "OutFile `"infirmary-integrated-win.exe`""
        $replace = "OutFile `"infirmary-integrated-$version-win-$arch.exe`""
        $outNSIS = "infirmary-integrated-$version-win-$arch.exe"
        (Get-Content $tempNSIS).replace($find, $replace) | Set-Content $tempNSIS

        # Compile installable .exe via NSIS
        Write-Host ">> Compiling NSIS package to installation .exe..."
        Invoke-Expression "& '$pathNSIS' '$tempNSIS'"

        # Copy installable .exe back to Release
        Write-Host ">> Moving compiled NSIS .exe package to original folder."
        $outFile = Join-Path $pathProj $outNSIS
        Copy-Item $outFile -Destination $pathOrig
        $outFile = Join-Path $pathOrig $outNSIS
        Write-Host ">> Compiled installation executable located at $outFile" -ForegroundColor Green

        # Delete working directory
        Write-Host ">> Removing temporary working directory."
        Remove-Item -LiteralPath $pathTemp -Force -Recurse
        Write-Host ""
    }
}


# Process each .zip file in Release with -win- flag in filename
foreach ($file in Get-ChildItem -Path $pathRelease -Include *-win-*.exe -Name) {
    $file = Join-Path $pathRelease $file
    $l = Split-Path $file -Leaf

    Write-Host "Signing package $l using signtool.exe." -ForegroundColor Blue
    Write-Host ">> Reminder: security cards are blocked on RDP connections!" -ForegroundColor Yellow
    Invoke-Expression "& '$pathSigntool' sign /n `"Open Source Developer, Ibi Keller`" /t `"http://time.certum.pl`" /fd SHA256 /v '$file'"

    Write-Host "Checking signature for $l" -ForegroundColor Blue
    Invoke-Expression "& '$pathSigntool' verify /v /pa '$file'"
    Write-Host ""
}