namespace RepositoryLinter.Checks;

/// <summary>
/// Check if a Directory exists in the repository
/// </summary>
/// <param name="relativeDirectoryPath">Relative path from the Git repository.</param>
/// <param name="pathToGitDirectory">Path to the local copy of the Git repository.</param>
public class DirectoryExistsCheck(string relativeDirectoryPath, string pathToGitDirectory) : Checker
{
    public CheckStatus StatusWhenEmpty { get; init; } = CheckStatus.Green;
    public override void Run()
    {
        var exists = Directory.Exists(Path.Join(pathToGitDirectory,relativeDirectoryPath));

        if (exists)
        {
            var empty = Directory.GetFiles(Path.Join(pathToGitDirectory, relativeDirectoryPath)).Length != 0;
            
            if (empty)
            {
                Status = StatusWhenEmpty;
                return;
            }
        }
        
        Status = exists ? CheckStatus.Green : StatusWhenFailed;
    }
}