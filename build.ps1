# Globals
$RootDir            = "$PSScriptRoot"
$NugetDir           = "$RootDir/.nuget"
$NugetExe           = "$NugetDir/NuGet.exe"
$PackagesConfigFile = "$NugetDir/packages.config"
$LibraryDir			= "$RootDir/Library"
$PackagesDir        = "$LibraryDir/NuGet"
$CakeVersionXPath   = "//package[@id='Cake'][1]/@version"
$CakeVersion        = (Select-Xml -Xml ([xml](Get-Content $PackagesConfigFile)) -XPath $CakeVersionXPath).Node.Value
$CakeExe            = "$PackagesDir/Cake.$CakeVersion/Cake.exe"

# Install build packages
iex "$NugetExe install `"$PackagesConfigFile`" -OutputDirectory `"$PackagesDir`"" |
	Select-String -NotMatch -Pattern "All packages listed in packages.config are already installed."

# Run Cake
iex "$CakeExe $args"
exit $LASTEXITCODE
