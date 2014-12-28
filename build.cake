#l "utilities.cake"

using YamlDotNet.Serialization;

public sealed class BuildConfiguration
{
    [YamlAlias("ksp_dir")]
    public string KspDir { get; set; }

    [YamlAlias("ksp_bin")]
    public string KspBin { get; set; }
}

var target = Argument<string>("target");
var configuration = Argument<string>("configuration", "Debug");

var buildConfiguration = GetBuildConfiguration<BuildConfiguration>();

var outputDirectory = "Output";
var binDirectory = System.IO.Path.Combine(outputDirectory, "Plugins", "bin", configuration);
var stageDirectory = System.IO.Path.Combine(outputDirectory, "Stage", configuration);
var stageGameDataDirectory = System.IO.Path.Combine(stageDirectory, "GameData");
var stageAirplaneModeDirectory = System.IO.Path.Combine(stageGameDataDirectory, "AeroplaneMode");
var deployAirplaneModeDirectory = System.IO.Path.Combine(buildConfiguration.KspDir, "GameData", "AeroplaneMode");
var packageDirectory = System.IO.Path.Combine(outputDirectory, "Package", configuration);

Task("Init")
    .Does(() =>
{
    var kspLibDirectory = System.IO.Path.Combine("Library", "KSP");
    var kspLibs = new [] { "Assembly-CSharp.dll", "Assembly-CSharp-firstpass.dll", "UnityEngine.dll" };

    CreateDirectory(kspLibDirectory);

    foreach (var kspLib in kspLibs)
    {
        if (!File.Exists(System.IO.Path.Combine(kspLibDirectory, kspLib)))
        {
            CopyFileToDirectory(
                System.IO.Path.Combine(buildConfiguration.KspDir, "KSP_Data", "Managed", kspLib),
                kspLibDirectory
            );
        }
    }
});

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
    .IsDependentOn("Init")
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

Task("Run")
    .IsDependentOn("Deploy")
    .Does(() =>
{
    StartProcess(System.IO.Path.Combine(buildConfiguration.KspDir, buildConfiguration.KspBin), new ProcessSettings
        {
            WorkingDirectory = buildConfiguration.KspDir
        });
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
