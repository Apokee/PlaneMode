Param (
    [Parameter(Position = 0)]
    [string]$Arg0,

    [Parameter(ValueFromRemainingArguments = $true)]
    [Object[]]$RemainingArgs
)

# Globals
$NugetVersion       = "4.1.0"
$UseExperimental    = $false
$RootDir            = "${PSScriptRoot}"
$BuildDir           = "${RootDir}/.build"
$ToolsDir           = "${BuildDir}/tools"
$PackagesDir        = "${BuildDir}/lib/nuget"
$NugetExe           = "${ToolsDir}/nuget/${NugetVersion}/NuGet.exe"
$PackagesConfigFile = "${RootDir}/packages.config"
$CakeVersion        = (Select-Xml -Xml ([xml](Get-Content $PackagesConfigFile)) -XPath "//package[@id='Cake'][1]/@version").Node.Value
$CakeExe            = "${PackagesDir}/Cake.${CakeVersion}/Cake.exe"

# Download NuGet
$nugetDir = Split-Path $NugetExe -Parent
if (!(Test-Path $nugetDir)) {
    mkdir $nugetDir > $null
}

if (!(Test-Path $NugetExe)) {
    (New-Object System.Net.WebClient).DownloadFile("https://dist.nuget.org/win-x86-commandline/v${NugetVersion}/nuget.exe", $NugetExe)
}

# Install build packages
iex "${NugetExe} install `"${PackagesConfigFile}`" -OutputDirectory `"${PackagesDir}`""

# Build args
$cakeArgs = @()

if ($Arg0) {
    if ($Arg0[0] -eq "-") {
        $cakeArgs += "${Arg0}"
    } else {
        $cakeArgs += "--target=${Arg0}"
    }
}

if ($UseExperimental) {
    $cakeArgs += "--experimental"
}

# Run Cake
iex "${CakeExe} ${cakeArgs} ${RemainingArgs}"
exit $LASTEXITCODE
