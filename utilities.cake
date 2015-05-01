#r "Library/NuGet/YamlDotNet.3.5.1/lib/net35/YamlDotNet.dll"

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
