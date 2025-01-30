namespace RepositoryLinter.Checks;

/// <summary>
/// Check if a file exists in the repository
/// </summary>
/// <param name="relativeFilePath">Relative file path from the Git repository. Supports wildcards.</param>
/// <param name="pathToGitDirectory">Path to the local copy of the Git repository.</param>
public class FileExistsCheck(string relativeFilePath, string pathToGitDirectory) : Checker
{
    public override void Run()
    {
        var fileName = Path.GetFileName(relativeFilePath);
        var directory = Path.GetDirectoryName(relativeFilePath);

        var exists = Directory.EnumerateFiles(Path.Join(pathToGitDirectory, directory), fileName, SearchOption.TopDirectoryOnly)
            .Any();
        
        Status = exists ? CheckStatus.Green : CheckStatus.Red;

    }
    
    public override string ToString()
    {
        return Status == CheckStatus.Green ? $"{CheckStatusExtensions.ToIcon(Status)} {Name}" : $"{CheckStatusExtensions.ToIcon(Status)} {Name}\nDescription: {Description}\nTip to fix: {TipToFix}";
    }
}