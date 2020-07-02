<#
.SYNOPSIS
    Copies the provided MSI location to the root of the repo for easy access, and renames the MSI to include the version number with it.
.EXAMPLE
    ./copy-msi-with-rename.ps1 "MSI-Location" "Output-Directory-To-Place-The-MSI"
.EXAMPLE
    ./copy-msi-with-rename.ps1 "C:\Code\MyRepo\InstallerProject\bin\Debug\MyInstaller.msi" "C:\Code\Project"
#>

param(
    [string]$msiLocation=$(throw "MSI location is required"),
    [string]$exeLocation=$(throw "EXE location is required"),
    [string]$outputDirectory=$(throw "Output directory is required")
)

# Initialize internal variables
$versionToAssign = [System.Version]::Parse("0.0.0.1") # default to very early value

Write-Host "RECEIVED DATA"
Write-Host $msiLocation
Write-Host $exeLocation
Write-Host $outputDirectory

# Validate parameters and expected paths
If (![System.IO.File]::Exists($msiLocation)){
    Write-Error "MSI file not found! -- " + $msiLocation
    Exit 2
}
If (!$msiLocation.EndsWith("msi")){
    Write-Error "MSI file is not a MSI -- $msiLocation"
    Exit 2
}
If (![System.IO.File]::Exists($exeLocation)){
    Write-Error "EXE file not found! -- $exeLocation"
    Exit 3
}
If (!$exeLocation.EndsWith("exe")){
    Write-Error "EXE file is not a EXE -- $exeLocation"
    Exit 3
}
Write-Debug "Received $outputDirectory"
$parseMe = $outputDirectory
If ($parseMe.EndsWith("..")){
    $parseMe = $parseMe -join '\'
}
Write-Debug "Going to parse $parseMe"
$destinationDirectory = [System.IO.Path]::GetFullPath($parseMe)
Write-Debug "destinationDirectory = '$destinationDirectory'"
If (![System.IO.Directory]::Exists($destinationDirectory)){
    Write-Error "Output directory does not exist -- '$destinationDirectory'"
    Exit 3
}

# Grab version off known EXE location
try {
    $versionToAssign = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($exeLocation)
}
catch [System.Exception] {
    $ex = $_.Exception
    Write-Warning "Exception thrown trying to get version, going to use default assignment instead -- details: " + $ex
}

# Determine destination file location
$newFileName = "PowerChangeAlerter-" + $versionToAssign.ProductVersion + [System.IO.Path]::GetExtension($msiLocation)
Write-Debug "New filename = $newFileName"
$destinationFileLocation = [System.IO.Path]::Combine($destinationDirectory, $newFileName)
Write-Debug "destinationFileLocation = $destinationFileLocation"

# Remove all MSI files from output directory
$deleteList = [System.IO.Directory]::GetFiles($destinationDirectory, "*.msi")
if ($deleteList.Count -gt 0) {
    Write-Warning "Found $($deleteList.Count) file(s) to delete in output location, going to delete them"
    $deleteList | ForEach-Object { [System.IO.File]::Delete($_) }
}

# Copy source MSI to output directory with adjusted filename
[System.IO.File]::Copy($msiLocation, $destinationFileLocation)

Write-Host "Copied MSI to here: $destinationFileLocation"
Exit 0