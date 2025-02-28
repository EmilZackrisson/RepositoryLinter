using System.Text;

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
    public CheckStatus StatusWhenEmpty { get; init; } = CheckStatus.Red;
    private string _additionalInfo = string.Empty;
    public List<string> ShouldContainFiles { get; init; } = [];
    public SearchOption SearchOption { get; init; } = SearchOption.TopDirectoryOnly;
    private readonly List<string> _directoryContent = [];
    
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

                if (ShouldContainFiles.Count != 0)
                {
                    foreach (var file in ShouldContainFiles)
                    {
                        _directoryContent.AddRange(Directory.GetFiles(path, file, SearchOption));
                    }

                    if (_directoryContent.Count == 0)
                    {
                        Status = StatusWhenEmpty;
                        _additionalInfo = $"Directory {path} does not contain any of {string.Join(',', ShouldContainFiles)}";
                        return;
                    }
                }
            }
            
            Status = exists ? CheckStatus.Green : StatusWhenFailed;
    }

    public override string ToString()
    {
        var builder = new StringBuilder(base.ToString());
        
        if (_additionalInfo != string.Empty)
        {
            builder.Append(Environment.NewLine);
            builder.Append(_additionalInfo);
        }

        if (_directoryContent.Count == 0) return builder.ToString();
        
        foreach (var file in _directoryContent)
        {
            builder.Append(Environment.NewLine);
            builder.Append(Path.GetRelativePath(pathToGitDirectory, file));
        }

        return builder.ToString();
    }
}