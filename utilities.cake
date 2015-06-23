#r "Library/NuGet/YamlDotNet.3.6.0/lib/net35/YamlDotNet.dll"

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

public void PathMSBuild(FilePath solution, string configuration)
{
    var exitCode = StartProcess(
        @"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe",
        new ProcessSettings
        {
            Arguments = String.Format(@"{0} /p:Configuration={1}", solution.FullPath, configuration)
        }  
    );
    
    if (exitCode != 0)
    {
        throw new Exception("msbuild build failed.");
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
