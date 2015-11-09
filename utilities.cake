#r "Library/NuGet/YamlDotNet.3.7.0/lib/net35/YamlDotNet.dll"

using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

public T GetBuildConfiguration<T>() where T : new()
{
    var workingDirectorySegments = Context.Environment.WorkingDirectory.Segments;
    var workingDirectoryName = workingDirectorySegments[workingDirectorySegments.Length - 1];

    var configFile = (new [] { "build.yml", String.Format("../{0}.build.yml", workingDirectoryName) })
        .FirstOrDefault(System.IO.File.Exists);

    if (configFile == null)
    {
        return new T();
    }

    return new Deserializer(ignoreUnmatched: true).Deserialize<T>(new StreamReader(configFile));
}

public string GetSolution()
{
    var solutions = System.IO.Directory.GetFiles(Context.Environment.WorkingDirectory.FullPath, "*.sln");

    if (solutions.Length == 1)
    {
        return solutions[0];
    }
    else
    {
        if (solutions.Length == 0)
        {
            throw new Exception("No solution found.");
        }
        else
        {
            throw new Exception("Multiple solutions found.");
        }
    }
}

public string Which(string executable)
{
    char[] seperators = { System.IO.Path.PathSeparator };

    var envPath = Environment.GetEnvironmentVariable("PATH");
    var envPathExt = Environment.GetEnvironmentVariable("PATHEXT");

    var paths = envPath == null ?
        new string[0] :
        envPath.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

    var pathExts = envPathExt == null ?
        new string[0] :
        envPathExt.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

    foreach (var path in paths)
    {
        var testPath = System.IO.Path.Combine(path, executable);

        /* We test the extensionful version first since it's not uncommon for multiplatform programs to ship with a
         * Unix executable without an extension in the same directory as a Windows extension with an extension such as
         * .cmd, .bat. In those cases trying to execute the extensionless version will fail on Windows.
         */
        foreach (var pathExt in pathExts)
        {
            var testPathExt = System.IO.Path.Combine(path, executable) + pathExt;

            if (FileExists(testPathExt))
            {
                return testPathExt;
            }
        }

        if (FileExists(testPath))
        {
            return testPath;
        }
    }

    return null;
}

public string GetGitRevision(bool useShort)
{
    var git = Which("git");

    if (git != null)
    {
        IEnumerable<string> output;

        var shortOption = useShort ? "--short" : "";
        StartProcess(git,
            new ProcessSettings { RedirectStandardOutput = true, Arguments = $"rev-parse {shortOption} HEAD"},
            out output
        );

        var outputList = output.ToList();
        if (outputList.Count == 1)
        {
            return outputList[0];
        }
        else
        {
            throw new Exception("Could not read revision from git");
        }
    }

    return null;
}

public SemVer GetVersion()
{
    return GetChangeLog().LatestVersion;
}

public ChangeLog GetChangeLog()
{
    return new ChangeLog("CHANGES.md");
}

public sealed class ChangeLog
{
    private static readonly Regex VersionPattern = new Regex(@"^## v(?<version>.+)$", RegexOptions.Compiled);

    public SemVer LatestVersion { get; }
    public string LatestChanges { get; }

    public ChangeLog(string path)
    {
        var lines = System.IO.File.ReadAllLines(path);

        var latestChanges = new List<string>();

        if (lines.Any())
        {
            var versionMatch = VersionPattern.Match(lines[0]);

            if (versionMatch.Success)
            {
                LatestVersion = new SemVer(versionMatch.Groups["version"].Value);
            }
            else
            {
                throw new Exception("Changes file is in incorrect format.");
            }

            foreach (var line in lines.Skip(1))
            {
                if (!VersionPattern.IsMatch(line))
                {
                    latestChanges.Add(line);
                }
                else
                {
                    break;
                }
            }

            LatestChanges = string.Join("\n", latestChanges.ToArray());
        }
        else
        {
            throw new Exception("Changes file is empty");
        }
    }
}

public sealed class SemVer
{
    private static readonly Regex Pattern = new Regex(
        @"^(?<major>[1-9]\d*?|0)\.(?<minor>[1-9]\d*?|0)\.(?<patch>[1-9]\d*?|0)(?:-(?<pre>[\dA-Z-]+))?(?:\+(?<build>[\dA-Z-]+))?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    private string _string;

    public uint Major { get; }
    public uint Minor { get; }
    public uint Patch { get; }
    public string Pre { get; }
    public string Build { get; }

    public SemVer(string s)
    {
        _string = s.Trim();

        var match = Pattern.Match(_string);

        if (match.Success)
        {
            Major = uint.Parse(match.Groups["major"].Value);
            Minor = uint.Parse(match.Groups["minor"].Value);
            Patch = uint.Parse(match.Groups["patch"].Value);

            var preGroup = match.Groups["pre"];
            if (preGroup.Success)
            {
                Pre = preGroup.Value;
            }

            var buildGroup = match.Groups["build"];
            if (buildGroup.Success)
            {
                Build = buildGroup.Value;
            }
        }
        else
        {
            throw new FormatException($"Unable to parse semantic version: {_string}");
        }
    }

    public SemVer(uint major = 0, uint minor = 0, uint patch = 0, string pre = null, string build = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Pre = pre;
        Build = build;

        _string = $"{Major}.{Minor}.{Patch}";

        if (pre != null)
        {
            _string += $"-{Pre}";
        }

        if (build != null)
        {
            _string += $"+{Build}";
        }
    }

    public override string ToString()
    {
        return _string;
    }

    public static implicit operator string(SemVer version)
    {
        return version.ToString();
    }
}
