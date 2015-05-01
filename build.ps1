Param (
	[Parameter(Position = 0)]
	[string]$Target,

	[Switch]
	$NoExperimental,

	[Parameter(ValueFromRemainingArguments = $true)]
	[Object[]]$RemainingArgs
)

# Globals
# TODO: The experimental flag is necessary after upgrading to VS2015 until the final version of Roslyn is available
# This should presumably occur once VS2015 is final
$UseExperimental	= $true
$RootDir            = "$PSScriptRoot"
$NugetDir           = "$RootDir/.nuget"
$NugetExe           = "$NugetDir/NuGet.exe"
$PackagesConfigFile = "$NugetDir/packages.config"
$LibraryDir         = "$RootDir/Library"
$PackagesDir        = "$LibraryDir/NuGet"
$CakeVersionXPath   = "//package[@id='Cake'][1]/@version"
$CakeVersion        = (Select-Xml -Xml ([xml](Get-Content $PackagesConfigFile)) -XPath $CakeVersionXPath).Node.Value
$CakeExe            = "$PackagesDir/Cake.$CakeVersion/Cake.exe"

if ($NoExperimental.IsPresent) {
	$UseExperimental = $false
}

# Install build packages
iex "$NugetExe install `"$PackagesConfigFile`" -OutputDirectory `"$PackagesDir`"" |
	Select-String -NotMatch -Pattern "All packages listed in packages.config are already installed."

# Build args
$cakeArgs = @()

if ($Target) {
	$cakeArgs += "-target=$Target"
}

if ($UseExperimental) {
	$cakeArgs += "-experimental"
}

# Run Cake
iex "$CakeExe $cakeArgs $RemainingArgs"
exit $LASTEXITCODE
