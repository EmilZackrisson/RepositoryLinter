namespace RepositoryLinter.Checks;

/// <summary>
/// Check if a Directory exists in the repository
/// </summary>
/// <param name="relativeDirectoryPath">Relative path from the Git repository.</param>
/// <param name="pathToGitDirectory">Path to the local copy of the Git repository.</param>
public class DirectoryExistsCheck(string relativeDirectoryPath, string pathToGitDirectory) : Checker
{
    /// <summary>
    /// Status to return when the directory is empty. Default is Green.
    /// </summary>
    public CheckStatus StatusWhenEmpty { get; init; } = CheckStatus.Green;
    private string _additionalInfo = string.Empty;
    public override void Run()
    {
        var path = Path.Join(pathToGitDirectory, relativeDirectoryPath);
        var exists = Directory.Exists(path);

        if (exists)
        {
            var empty = Directory.EnumerateFiles(path).ToList().Count == 0;
            
            if (empty)
            {
                Status = StatusWhenEmpty;
                _additionalInfo = $"Directory {relativeDirectoryPath} is empty.";
                return;
            }
        }
        
        Status = exists ? CheckStatus.Green : StatusWhenFailed;
    }

    public override string ToString()
    {
        var b = base.ToString();
        if (_additionalInfo != string.Empty)
        {
            b += $"\n{_additionalInfo}";
        }
        
        return b;
    }
}