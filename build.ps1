Param (
    [Parameter(Position = 0)]
    [string]$Arg0,

    [Parameter(ValueFromRemainingArguments = $true)]
    [Object[]]$RemainingArgs
)

# Globals
# TODO: The experimental flag is necessary after upgrading to VS2015 until the final version of Roslyn is available
# This should presumably occur once VS2015 is final
$UseExperimental	= $true
$RootDir            = "$PSScriptRoot"
$PackagesConfigFile = "$RootDir/packages.config"
$PackagesDir        = "$RootDir/Library/NuGet"
$CakeVersionXPath   = "//package[@id='Cake'][1]/@version"
$CakeVersion        = (Select-Xml -Xml ([xml](Get-Content $PackagesConfigFile)) -XPath $CakeVersionXPath).Node.Value
$CakeExe            = "$PackagesDir/Cake.$CakeVersion/Cake.exe"

# Install build packages
iex "NuGet install `"$PackagesConfigFile`" -OutputDirectory `"$PackagesDir`"" |
    Select-String -NotMatch -Pattern "All packages listed in packages.config are already installed."

# Build args
$cakeArgs = @()

if ($Arg0) {
    if ($Arg0[0] -eq "-") {
        $cakeArgs += "$Arg0"
    } else {
        $cakeArgs += "-target=$Arg0"
    }
}

if ($UseExperimental) {
    $cakeArgs += "-experimental"
}

# Run Cake
iex "$CakeExe $cakeArgs $RemainingArgs"
exit $LASTEXITCODE
