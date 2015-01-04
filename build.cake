#l "utilities.cake"

using YamlDotNet.Serialization;

public sealed class BuildConfiguration
{
    [YamlAlias("ksp_dir")]
    public string KspDir { get; set; }

    [YamlAlias("ksp_bin")]
    public string KspBin { get; set; }

    public string KspPath(params string[] paths)
    {
        return KspDir == null ? null : System.IO.Path.Combine(KspDir, System.IO.Path.Combine(paths));
    }
}

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Debug");

var buildConfiguration = GetBuildConfiguration<BuildConfiguration>();

var outputDirectory = "Output";
var buildDirectory = System.IO.Path.Combine(outputDirectory, "Build", configuration);
var binDirectory = System.IO.Path.Combine(buildDirectory, "Common", "bin");
var stageDirectory = System.IO.Path.Combine(outputDirectory, "Stage", configuration);
var stageGameDataDirectory = System.IO.Path.Combine(stageDirectory, "GameData");
var stageAirplaneModeDirectory = System.IO.Path.Combine(stageGameDataDirectory, "AirplaneMode");
var deployAirplaneModeDirectory = buildConfiguration.KspPath("GameData", "AirplaneMode");
var packageDirectory = System.IO.Path.Combine(outputDirectory, "Package", configuration);

Task("Default")
    .IsDependentOn("Stage")
    .Does(() => { });

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
                buildConfiguration.KspPath("KSP_Data", "Managed", kspLib),
                kspLibDirectory
            );
        }
    }
});

Task("CleanBuild")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] { buildDirectory });
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
    .IsDependentOn("CleanBuild")
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
    var pluginsDirectory = System.IO.Path.Combine(stageAirplaneModeDirectory, "Plugins");
    var texturesDirectory = System.IO.Path.Combine(stageAirplaneModeDirectory, "Textures");

    var artworkDirectory = GetNuGetPackageDirectory("Apokee.Artwork");

    CreateDirectory(stageGameDataDirectory);
    CreateDirectory(stageAirplaneModeDirectory);
    CreateDirectory(pluginsDirectory);
    CreateDirectory(texturesDirectory);

    CopyFiles(binDirectory + "/*", pluginsDirectory);
    CopyFiles("GameData/*", stageAirplaneModeDirectory);
    CopyFile(
        System.IO.Path.Combine(artworkDirectory, "Content", "airplane-white-38x38.png"),
        System.IO.Path.Combine(stageAirplaneModeDirectory, "Textures", "AppLauncherAirplane.png")
    );
    CopyFile(
        System.IO.Path.Combine(artworkDirectory, "Content", "rocket-white-38x38.png"),
        System.IO.Path.Combine(stageAirplaneModeDirectory, "Textures", "AppLauncherRocket.png")
    );
    CopyFileToDirectory("CHANGES.md", stageAirplaneModeDirectory);
    CopyFileToDirectory("LICENSE", stageAirplaneModeDirectory);
    CopyFileToDirectory("README.md", stageAirplaneModeDirectory);
});

Task("Deploy")
    .IsDependentOn("Stage")
    .IsDependentOn("CleanDeploy")
    .Does(() =>
{
    CopyDirectory(stageAirplaneModeDirectory, buildConfiguration.KspPath("GameData"));
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

public string GetNuGetPackageDirectory(string package)
{
    return Directory
        .GetDirectories("Library/NuGet")
        .Select(i => new DirectoryInfo(i))
        .Where(i => i.Name.StartsWith(package))
        .OrderByDescending(i => new Version(i.Name.Substring(package.Length + 1, i.Name.Length - package.Length - 1)))
        .First()
        .FullName;
}

RunTarget(target);
