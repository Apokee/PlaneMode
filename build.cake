#l "utilities.cake"

var target = Argument<string>("target", "Build");
var configuration = Argument<string>("configuration", "Debug");

var outputDirectory = "Output";
var binDirectory = System.IO.Path.Combine(outputDirectory, "Plugins", "bin", configuration);
var stageDirectory = System.IO.Path.Combine(outputDirectory, "Stage", configuration);
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
    var gameDataDirectory = System.IO.Path.Combine(stageDirectory, "GameData");
    var airplaneModeDirectory = System.IO.Path.Combine(gameDataDirectory, "AeroplaneMode");
    var pluginsDirectory = System.IO.Path.Combine(airplaneModeDirectory);

    CreateDirectory(gameDataDirectory);
    CreateDirectory(airplaneModeDirectory);
    CreateDirectory(pluginsDirectory);

    CopyFiles(binDirectory + "/*", pluginsDirectory);
    CopyFiles("GameData/*", airplaneModeDirectory);
    CopyFileToDirectory("CHANGES.md", airplaneModeDirectory);
    CopyFileToDirectory("LICENSE", airplaneModeDirectory);
    CopyFileToDirectory("README.md", airplaneModeDirectory);
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
