#r "Library/NuGet/YamlDotNet.3.5.0/lib/net35/YamlDotNet.dll"

using YamlDotNet.Serialization;

public T GetBuildConfiguration<T>() where T : new()
{
    var workingDirectorySegments = GetContext().Environment.WorkingDirectory.Segments;
    var workingDirectoryName = workingDirectorySegments[workingDirectorySegments.Length - 1];

    var configFile = (new [] { "build.yml", String.Format("../{0}.build.yml", workingDirectoryName) })
        .FirstOrDefault(File.Exists);

    if (configFile == null)
    {
        return new T();
    }

    return new Deserializer(ignoreUnmatched: true).Deserialize<T>(new StreamReader(configFile));
}

public string GetSolution()
{
    var solutions = Directory.GetFiles(GetContext().Environment.WorkingDirectory.FullPath, "*.sln");

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

public void CopyDirectory(string directory, string targetParentDirectory)
{
    var directoryInfo = new DirectoryInfo(directory);
    var targetParentDirectoryInfo = new DirectoryInfo(targetParentDirectory);

    var targetDirectory = System.IO.Path.Combine(targetParentDirectory, directoryInfo.Name);
    if (!Directory.Exists(targetDirectory))
    {
        Directory.CreateDirectory(targetDirectory);
    }

    foreach(var childFileInfo in directoryInfo.EnumerateFiles())
    {
        childFileInfo.CopyTo(System.IO.Path.Combine(targetDirectory, childFileInfo.Name));
    }

    foreach (var childDirectoryInfo in directoryInfo.EnumerateDirectories())
    {
        CopyDirectory(childDirectoryInfo.FullName, targetDirectory);
    }
}
