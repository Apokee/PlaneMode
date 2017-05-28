#addin "nuget:?package=Cake.FileHelpers&version=1.0.4"
#l "cake/utilities.cake"

using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

public sealed class BuildConfiguration
{
    [YamlMember(Alias = "ksp_dir")]
    public string KspDir { get; set; }

    [YamlMember(Alias = "ksp_bin")]
    public string KspBin { get; set; }

    public string KspPath(params string[] paths)
    {
        return KspDir == null ? null : System.IO.Path.Combine(KspDir, System.IO.Path.Combine(paths));
    }
}

public sealed class Globals
{
    public string ModIdentifier { get; set; }
    public string ModName { get; set; }
    public string ModCopyrightDates { get; set; }
    public string[] KspLibs { get; set; }
    public string Target { get; set; }
    public string Configuration { get; set; }
    public BuildConfiguration BuildConfiguration { get; set; }
    public SemVer BuildVersion { get; set; }
    public FilePath Solution { get; set; }
    public DirectoryPath RootDirectory { get; set; }
    public DirectoryPath BuildDirectory { get; set; }
    public DirectoryPath BuildLibDirectory { get; set; }
    public DirectoryPath BuildLibKspDirectory { get; set; }
    public DirectoryPath BuildLibNugetDirectory { get; set; }
    public DirectoryPath BuildMetaDirectory { get; set; }
    public DirectoryPath BuildPkgDirectory { get; set; }
    public DirectoryPath BuildStageDirectory { get; set; }
    public DirectoryPath BuildStageGameDataDirectory { get; set; }
    public DirectoryPath BuildStageGameDataModDirectory { get; set; }
    public DirectoryPath BuildOutDirectory { get; set; }
    public DirectoryPath SrcDirectory { get; set; }
    public DirectoryPath DeployDirectory { get; set; }
}

// HACK: Terrible workaround for Mono's script compiler not supporting globals like Roslyn
private Globals GetGlobals()
{
    var globals = new Globals();

    globals.ModIdentifier = "PlaneMode";
    globals.ModName = "Plane Mode";
    globals.ModCopyrightDates = "2015-2016";
    globals.KspLibs = new []
    {
        "Assembly-CSharp.dll",
        "Assembly-CSharp-firstpass.dll",
        "UnityEngine.dll",
        "UnityEngine.UI.dll"
    };
    globals.Target = Argument<string>("target", "Package");
    globals.Configuration = Argument<string>("configuration", "Debug");
    globals.BuildConfiguration = GetBuildConfiguration<BuildConfiguration>();
    globals.BuildVersion = GetBuildVersion();
    globals.Solution = GetSolution();
    globals.RootDirectory = Context.Environment.WorkingDirectory;
    globals.BuildDirectory = globals.RootDirectory.Combine(".build");
    globals.BuildLibDirectory = globals.BuildDirectory.Combine("lib");
    globals.BuildLibKspDirectory = globals.BuildLibDirectory.Combine("ksp");
    globals.BuildLibNugetDirectory = globals.BuildLibDirectory.Combine("nuget");
    globals.BuildMetaDirectory = globals.BuildDirectory.Combine("meta");
    globals.BuildPkgDirectory = globals.BuildDirectory.Combine("pkg").Combine(globals.Configuration);
    globals.BuildStageDirectory = globals.BuildDirectory.Combine("stage");
    globals.BuildStageGameDataDirectory = globals.BuildStageDirectory.Combine("GameData");
    globals.BuildStageGameDataModDirectory = globals.BuildStageGameDataDirectory.Combine(globals.ModIdentifier);
    globals.BuildOutDirectory = globals.BuildDirectory.Combine("out");
    globals.SrcDirectory = globals.RootDirectory.Combine("src");
    
    var deployDirectory = globals.BuildConfiguration.KspPath("GameData", globals.ModIdentifier);    
    if (deployDirectory != null) { globals.DeployDirectory =  deployDirectory; }

    return globals;
}

Task("Init")
    .IsDependentOn("InitLibKsp");

Task("InitLibKsp")
    .Does(() =>
{
    const string kspLibsUrlBase = "http://build.apokee.com/dependencies/ksp/1.2.0.1586";

    var globals = GetGlobals();
    
    CreateDirectory(globals.BuildLibKspDirectory);
    
    var missingKspLibs = globals.KspLibs.Where(i => !FileExists(globals.BuildLibKspDirectory.CombineWithFilePath(i)));
    foreach (var kspLib in missingKspLibs)
    {
        var destinationKspLibFilePath = globals.BuildLibKspDirectory.CombineWithFilePath(kspLib);
        var localKspLib = globals.BuildConfiguration.KspPath("KSP_x64_Data", "Managed", kspLib);

        if (localKspLib != null && FileExists(localKspLib))
            CopyFile(localKspLib, destinationKspLibFilePath);
        else
            DownloadFile(string.Format("{0}/{1}", kspLibsUrlBase, kspLib), destinationKspLibFilePath);
    }
});

Task("CleanStage")
    .Does(() =>
{
    var globals = GetGlobals();
    
    CleanDirectories(new [] { globals.BuildStageDirectory }); 
});

Task("CleanPackage")
    .Does(() =>
{
    var globals = GetGlobals();
    
    CleanDirectories(new [] { globals.BuildPkgDirectory }); 
});

Task("CleanDeploy")
    .Does(() =>
{
    var globals = GetGlobals();
    
    CleanDirectories(new [] {globals.DeployDirectory });
});

Task("Restore")
    .Does(() =>
{
    var globals = GetGlobals();
    
    NuGetRestore(globals.Solution);
});

Task("BuildMeta")
    .IsDependentOn("BuildAssemblyInfo")
    .Does(() =>
{
    var globals = GetGlobals();
    
    CreateDirectory(globals.BuildMetaDirectory);
    
    FileWriteText(
        globals.BuildMetaDirectory.CombineWithFilePath("VERSION"),
        globals.BuildVersion
    );
    FileWriteText(
        globals.BuildMetaDirectory.CombineWithFilePath("PRERELEASE"),
        (globals.BuildVersion.Pre != null).ToString().ToLowerInvariant()
    );
    FileWriteText(
        globals.BuildMetaDirectory.CombineWithFilePath("CHANGELOG"),
        GetChangeLog().LatestChanges
    );
});

Task("BuildAssemblyInfo")
    .IsDependentOn("BuildGlobalAssemblyVersionInfo")
    .IsDependentOn("BuildGlobalKspAssemblyVersionInfo");

Task("BuildGlobalAssemblyVersionInfo")
    .Does(() =>
{
    var globals = GetGlobals();
    
    CreateDirectory(globals.BuildMetaDirectory);
    
    CreateAssemblyInfo(globals.BuildMetaDirectory.CombineWithFilePath("GlobalAssemblyVersionInfo.cs"), new AssemblyInfoSettings
    {
        Version = string.Format("{0}.{1}", globals.BuildVersion.Major, globals.BuildVersion.Minor),
        FileVersion = string.Format("{0}.{1}.{2}", globals.BuildVersion.Major, globals.BuildVersion.Minor, globals.BuildVersion.Patch),
        InformationalVersion = globals.BuildVersion.ToString()
    });
});

Task("BuildGlobalKspAssemblyVersionInfo")
    .Does(() =>
{
    var globals = GetGlobals();
    
    CreateDirectory(globals.BuildMetaDirectory);
    
    var sb = new StringBuilder();
    sb.AppendLine("//------------------------------------------------------------------------------");
    sb.AppendLine("// <auto-generated>");
    sb.AppendLine("//     This code was generated by Cake.");
    sb.AppendLine("// </auto-generated>");
    sb.AppendLine("//------------------------------------------------------------------------------");
    sb.AppendLine();
    sb.AppendLine(
        string.Format(@"[assembly: KSPAssembly(""{0}"", {1}, {2})]",
            globals.ModIdentifier,
            globals.BuildVersion.Major,
            globals.BuildVersion.Minor
        )
    );
     
    FileWriteText(globals.BuildMetaDirectory.CombineWithFilePath("GlobalAssemblyKspVersionInfo.cs"), sb.ToString());
});

Task("Build")
    .IsDependentOn("Init")
    .IsDependentOn("Restore")
    .IsDependentOn("BuildMeta")
    .Does(() =>
{
    var globals = GetGlobals();
    
    DotNetBuild(globals.Solution, s => { s.Configuration = globals.Configuration; });
});

Task("Stage")
    .IsDependentOn("CleanStage")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .Does(() =>
{
    var globals = GetGlobals();
    
    var artworkDirectory = GetNuGetPackageDirectory("Apokee.Artwork").Combine("Content");
    var binDirectory = globals
        .BuildOutDirectory
        .Combine(globals.ModIdentifier)
        .Combine(globals.Configuration)
        .Combine("bin");
    var srcGameDataDirectory = globals.SrcDirectory.Combine("GameData");
    
    var buildStageGameDataModPluginsDirectory = globals.BuildStageGameDataModDirectory.Combine("Plugins");
    var buildStageGameDataModTexturesDirectory = globals.BuildStageGameDataModDirectory.Combine("Textures");

    CreateDirectory(globals.BuildStageGameDataDirectory);
    CreateDirectory(globals.BuildStageGameDataModDirectory);
    CreateDirectory(buildStageGameDataModPluginsDirectory);
    CreateDirectory(buildStageGameDataModTexturesDirectory);

    CopyFiles(
        GetFiles(string.Format("{0}/*", binDirectory)),
        buildStageGameDataModPluginsDirectory
    );
    CopyDirectory(
        srcGameDataDirectory,
        globals.BuildStageGameDataModDirectory
    );
    CopyFile(
        artworkDirectory.CombineWithFilePath("airplane-white-38x38.png"),
        buildStageGameDataModTexturesDirectory.CombineWithFilePath("AppLauncherPlane.png")
    );
    CopyFile(
        artworkDirectory.CombineWithFilePath("rocket-white-38x38.png"),
        buildStageGameDataModTexturesDirectory.CombineWithFilePath("AppLauncherRocket.png")
    );
    CopyFileToDirectory("CHANGELOG.md", globals.BuildStageGameDataModDirectory);
    CopyFileToDirectory("LICENSE.md", globals.BuildStageGameDataModDirectory);
    CopyFileToDirectory("README.md", globals.BuildStageGameDataModDirectory);
});

Task("Deploy")
    .IsDependentOn("Stage")
    .IsDependentOn("CleanDeploy")
    .Does(() =>
{
    var globals = GetGlobals();
    
    CopyDirectory(globals.BuildStageGameDataModDirectory, globals.DeployDirectory);
});

Task("Run")
    .IsDependentOn("Deploy")
    .Does(() =>
{
    var globals = GetGlobals();
    
    StartAndReturnProcess(globals.BuildConfiguration.KspPath(globals.BuildConfiguration.KspBin), new ProcessSettings
    {
        WorkingDirectory = globals.BuildConfiguration.KspDir
    });
});

Task("Package")
    .IsDependentOn("CleanPackage")
    .IsDependentOn("Stage")
    .Does(() =>
{
    var globals = GetGlobals();
    
    CreateDirectory(globals.BuildPkgDirectory);

    Zip(
        globals.BuildStageDirectory,
        globals.BuildPkgDirectory.CombineWithFilePath(
            string.Format("{0}-{1}.zip", globals.ModIdentifier, globals.BuildVersion)
        )
    );
});

Task("Version")
    .Does(() =>
{    
    Information(GetVersion());
});

Task("ChangeLog")
    .Does(() =>
{    
    Information(GetChangeLog().LatestChanges);
});

RunTarget(GetGlobals().Target);

public SemVer GetBuildVersion()
{    
    SemVer buildVersion;

    var release = Argument<bool>("release", false);;
    var changeLog = GetChangeLog();
    var version = changeLog.LatestVersion;
    var rev = GetGitRevision(useShort: true);

    if (rev != null && !release)
    {
        if (version.Build == null)
            buildVersion = new SemVer(version.Major, version.Minor, version.Patch, version.Pre, rev);
        else
            throw new Exception("VERSION already contains build metadata");
    }
    else
    {
        buildVersion = version;
    }
    
    return buildVersion;
}

private DirectoryPath GetNuGetPackageDirectory(string package)
{
    var globals = GetGlobals();
    
    return GetDirectories(string.Format("{0}/*", globals.BuildLibNugetDirectory))
        .Where(i => i.GetDirectoryName().StartsWith(package))
        .OrderByDescending(i => {
            var directoryName = i.GetDirectoryName();
            return new Version(directoryName.Substring(package.Length + 1, directoryName.Length - package.Length - 1));
        })
        .FirstOrDefault();
}
