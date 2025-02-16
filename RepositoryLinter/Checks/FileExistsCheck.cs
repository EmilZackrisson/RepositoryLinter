namespace RepositoryLinter.Checks;

/// <summary>
/// Check if a file exists in the repository
/// </summary>
/// <param name="relativeFilePath">Relative file path from the Git repository. Supports wildcards. Not case-sensitive.</param>
/// <param name="pathToGitDirectory">Path to the local copy of the Git repository.</param>
public class FileExistsCheck(string relativeFilePath, string pathToGitDirectory) : Checker
{
    private bool _isEmpty;
    
    /// <summary>
    /// Status to return when the file is empty, aka 0 bytes. Default is Green.
    /// </summary>
    public CheckStatus StatusWhenEmpty { get; init; } = CheckStatus.Green;
    private string? _additionalInfo;
    
    /// <summary>
    /// Recursively search for the file in the directory. Default is false.
    /// </summary>
    public bool RecursiveSearch { get; init; }
    
    public override void Run()
    {
        var fileName = Path.GetFileName(relativeFilePath);
        var directory = Path.GetDirectoryName(relativeFilePath);

        List<string> files;
        
        if (RecursiveSearch)
        {
            files = Directory.EnumerateFiles(Path.Join(pathToGitDirectory, directory), fileName,
                SearchOption.AllDirectories).ToList();
        }
        else
        {
            files = Directory.EnumerateFiles(Path.Join(pathToGitDirectory, directory), fileName,
                SearchOption.TopDirectoryOnly).ToList();
        }
        
        var exists = files.Count != 0;
        
        if (files.Count > 1)
        {
            Status = CheckStatus.Yellow;
            _additionalInfo = $"\nMultiple files matching {fileName} found in the directory {directory}.";
            return;
        }
        
        if (exists)
        {
            _isEmpty = FilesAreEmpty(files);
        }
        
        if (exists && _isEmpty)
        {
            _additionalInfo += $"File {fileName} is empty.";
            Status = StatusWhenEmpty;
            return;
        }
        
        Status = exists ? CheckStatus.Green : StatusWhenFailed;
    }
    
    private static bool FileIsEmpty(string path)
    {
        try
        {
            return new FileInfo(path).Length == 0;
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e);
            return true;
        }
    }

    private static bool FilesAreEmpty(List<string> files)
    {
        return files.All(FileIsEmpty);
    }

    public override string ToString()
    {
        var str = base.ToString();
        
        if (_isEmpty)
        {
            str += "\nFile is empty";
        }
        
        if (_additionalInfo != "")
        {
            str += _additionalInfo;
        }
        
        return str;
    }
}