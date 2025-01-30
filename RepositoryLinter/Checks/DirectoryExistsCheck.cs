namespace RepositoryLinter.Checks;

/// <summary>
/// Check if a Directory exists in the repository
/// </summary>
/// <param name="relativeDirectoryPath">Relative path from the Git repository.</param>
/// <param name="pathToGitDirectory">Path to the local copy of the Git repository.</param>
public class DirectoryExistsCheck(string relativeDirectoryPath, string pathToGitDirectory) : Checker
{
    public override void Run()
    {
        var exists = Directory.Exists(Path.Join(pathToGitDirectory,relativeDirectoryPath));
        
        Status = exists ? CheckStatus.Green : StatusWhenFailed;
    }
}