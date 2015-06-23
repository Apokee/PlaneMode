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

var target = Argument<string>("target", "Package");
var configuration = Argument<string>("configuration", "Debug");
var release = Argument<bool>("release", false);

var buildConfiguration = GetBuildConfiguration<BuildConfiguration>();

var solution = GetSolution();

var identifier = "PlaneMode";
var outputDirectory = "Output";
var buildDirectory = System.IO.Path.Combine(outputDirectory, "Build", configuration);
var binDirectory = System.IO.Path.Combine(buildDirectory, "Common", "bin");
var stageDirectory = System.IO.Path.Combine(outputDirectory, "Stage", configuration);
var stageGameDataDirectory = System.IO.Path.Combine(stageDirectory, "GameData");
var stagePlaneModeDirectory = System.IO.Path.Combine(stageGameDataDirectory, identifier);
var deployPlaneModeDirectory = buildConfiguration.KspPath("GameData", identifier);
var packageDirectory = System.IO.Path.Combine(outputDirectory, "Package", configuration);

Task("Init")
    .Does(() =>
{
    var kspLibDirectory = System.IO.Path.Combine("Library", "KSP");
    var kspLibs = new [] { "Assembly-CSharp.dll", "Assembly-CSharp-firstpass.dll", "UnityEngine.dll" };

    CreateDirectory(kspLibDirectory);

    foreach (var kspLib in kspLibs)
    {
        if (!FileExists(System.IO.Path.Combine(kspLibDirectory, kspLib)))
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
    CleanDirectories(new DirectoryPath[] { deployPlaneModeDirectory });
});

Task("BuildVersionInfo")
    .Does(() =>
{
    SemVer buildVersion;

    var changeLog = GetChangeLog();
    var version = changeLog.LatestVersion;
    var rev = GetGitRevision(useShort: true);

    if (rev != null && !release)
    {
        if (version.Build == null)
        {
            buildVersion = new SemVer(version.Major, version.Minor, version.Patch, version.Pre, rev);
        }
        else
        {
            throw new Exception("VERSION already contains build metadata");
        }
    }
    else
    {
        buildVersion = version;
    }

    System.IO.File.WriteAllText("Output/VERSION", buildVersion);
    System.IO.File.WriteAllText("Output/PRELEASE", (buildVersion.Pre != null).ToString().ToLower());
    System.IO.File.WriteAllText("Output/CHANGELOG", changeLog.LatestChanges);
});

Task("BuildAssemblyInfo")
    .Does(() =>
{
    BuildAssemblyInfo($"Source/{identifier}/Properties/AssemblyInfo.cs");
});

Task("Build")
    .IsDependentOn("CleanBuild")
    .IsDependentOn("Init")
    .IsDependentOn("BuildVersionInfo")
    .IsDependentOn("BuildAssemblyInfo")
    .Does(() =>
{
    MSBuild(solution, s => { s.Configuration = configuration; });
});

Task("Stage")
    .IsDependentOn("CleanStage")
    .IsDependentOn("Build")
    .Does(() =>
{
    var pluginsDirectory = System.IO.Path.Combine(stagePlaneModeDirectory, "Plugins");
    var texturesDirectory = System.IO.Path.Combine(stagePlaneModeDirectory, "Textures");

    var artworkDirectory = GetNuGetPackageDirectory("Apokee.Artwork");

    CreateDirectory(stageGameDataDirectory);
    CreateDirectory(stagePlaneModeDirectory);
    CreateDirectory(pluginsDirectory);
    CreateDirectory(texturesDirectory);

    CopyFiles(binDirectory + "/*", pluginsDirectory);
    CopyDirectory("Patches", stagePlaneModeDirectory + "/Patches");
    CopyDirectory("Configuration", stagePlaneModeDirectory + "/Configuration");
    CopyFile(
        System.IO.Path.Combine(artworkDirectory, "Content", "airplane-white-38x38.png"),
        System.IO.Path.Combine(stagePlaneModeDirectory, "Textures", "AppLauncherPlane.png")
    );
    CopyFile(
        System.IO.Path.Combine(artworkDirectory, "Content", "rocket-white-38x38.png"),
        System.IO.Path.Combine(stagePlaneModeDirectory, "Textures", "AppLauncherRocket.png")
    );
    CopyFileToDirectory("CHANGES.md", stagePlaneModeDirectory);
    CopyFileToDirectory("LICENSE.md", stagePlaneModeDirectory);
    CopyFileToDirectory("README.md", stagePlaneModeDirectory);
});

Task("Deploy")
    .IsDependentOn("Stage")
    .IsDependentOn("CleanDeploy")
    .Does(() =>
{
    CopyDirectory(stagePlaneModeDirectory, buildConfiguration.KspPath("GameData") + $"/{identifier}");
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
    CreateDirectory(packageDirectory);

    var packageFile = System.IO.Path.Combine(
        packageDirectory,
        $"{identifier}-{GetBuildVersion()}.zip"
    );

    Zip(stageDirectory, packageFile);
});

RunTarget(target);

private void BuildAssemblyInfo(string file)
{
    var version = GetBuildVersion();

    var output = TransformTextFile($"{file}.in")
        .WithToken("VERSION", version)
        .WithToken("VERSION.MAJOR", version.Major)
        .WithToken("VERSION.MINOR", version.Minor)
        .WithToken("VERSION.PATCH", version.Patch)
        .WithToken("VERSION.PRE", version.Pre)
        .WithToken("VERSION.BUILD", version.Build)
        .ToString();

    System.IO.File.WriteAllText(file, output);
}

private string GetNuGetPackageDirectory(string package)
{
    return System.IO.Directory
        .GetDirectories("Library/NuGet")
        .Select(i => new DirectoryInfo(i))
        .Where(i => i.Name.StartsWith(package))
        .OrderByDescending(i => new Version(i.Name.Substring(package.Length + 1, i.Name.Length - package.Length - 1)))
        .First()
        .FullName;
}

private SemVer GetBuildVersion()
{
    return new SemVer(System.IO.File.ReadAllText("Output/VERSION"));
}
