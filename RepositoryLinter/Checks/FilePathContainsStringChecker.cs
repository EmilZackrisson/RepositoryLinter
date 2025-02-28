using System.Text;

namespace RepositoryLinter.Checks;

public class FilePathContainsStringChecker(string stringToFind, string pathToGitRepo) : Checker
{
    private readonly List<string> _foundPaths = [];

    public CheckStatus StatusWhenFound { get; init; } = CheckStatus.Green;
    public CheckStatus StatusWhenNotFound { get; init; } = CheckStatus.Red;

    private readonly GitIgnoreHandler _gitIgnore = new(pathToGitRepo);
    
    public override void Run()
    {
        var paths = GetAllFilePaths();
        
        foreach (var path in paths)
        {
            var relativePath = Path.GetRelativePath(pathToGitRepo, path);
            var found = relativePath.Contains(stringToFind);
            
            var ignored = _gitIgnore.IsIgnored(relativePath);

            if (found && !ignored)
            {
                _foundPaths.Add(relativePath);
            }
        }
        
        // Set status depending on if files are found
        Status = _foundPaths.Count != 0 ? StatusWhenFound : StatusWhenNotFound;
    }

    /// <summary>
    /// Gets all file paths recursively from git repo.
    /// </summary>
    /// <returns>A list of all paths</returns>
    private List<string> GetAllFilePaths()
    {
        return Directory.GetFiles(pathToGitRepo, "*.*", SearchOption.AllDirectories).ToList();
    }

    public override string ToString()
    {
        var builder = new StringBuilder(base.ToString());

        foreach (var path in _foundPaths)
        {
            builder.Append(Environment.NewLine);
            builder.Append(path);
        }
        
        return builder.ToString();
    }
}