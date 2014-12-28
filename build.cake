#l "utilities.cake"

var target = Argument<string>("target", "Build");
var configuration = Argument<string>("configuration", "Debug");

var outputDirectory = "Output";
var binDirectory = System.IO.Path.Combine(outputDirectory, "Plugins", "bin", configuration);
var stageDirectory = System.IO.Path.Combine(outputDirectory, "Stage", configuration);
var stageGameDataDirectory = System.IO.Path.Combine(stageDirectory, "GameData");
var stageAirplaneModeDirectory = System.IO.Path.Combine(stageGameDataDirectory, "AeroplaneMode");
var kspDirectory = "C:/Dev/Kerbal Space Program";
var deployAirplaneModeDirectory = System.IO.Path.Combine(kspDirectory, "GameData", "AeroplaneMode");
var packageDirectory = System.IO.Path.Combine(outputDirectory, "Package", configuration);

Task("Clean")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] { outputDirectory });
});

Task("CleanStage")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] { stageDirectory });
});

Task("CleanPackage")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] { packageDirectory });
});

Task("CleanDeploy")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] { deployAirplaneModeDirectory });
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    MSBuild(GetSolution(), settings => settings.SetConfiguration(configuration));
});

Task("Stage")
    .IsDependentOn("CleanStage")
    .IsDependentOn("Build")
    .Does(() =>
{
    var pluginsDirectory = System.IO.Path.Combine(stageAirplaneModeDirectory);

    CreateDirectory(stageGameDataDirectory);
    CreateDirectory(stageAirplaneModeDirectory);
    CreateDirectory(pluginsDirectory);

    CopyFiles(binDirectory + "/*", pluginsDirectory);
    CopyFiles("GameData/*", stageAirplaneModeDirectory);
    CopyFileToDirectory("CHANGES.md", stageAirplaneModeDirectory);
    CopyFileToDirectory("LICENSE", stageAirplaneModeDirectory);
    CopyFileToDirectory("README.md", stageAirplaneModeDirectory);
});

Task("Deploy")
    .IsDependentOn("Stage")
    .IsDependentOn("CleanDeploy")
    .Does(() =>
{
    CopyFiles(stageAirplaneModeDirectory + "/*", deployAirplaneModeDirectory);
});

Task("Package")
    .IsDependentOn("CleanPackage")
    .IsDependentOn("Stage")
    .Does(() =>
{
    var assemblyInfo = ParseAssemblyInfo("Source/AirplaneMode/Properties/AssemblyInfo.cs");

    CreateDirectory(packageDirectory);

    var packageFile = System.IO.Path.Combine(
        packageDirectory,
        "AirplaneMode-" + assemblyInfo.AssemblyInformationalVersion + ".zip"
    );

    Zip(stageDirectory, packageFile);
});

RunTarget(target);
