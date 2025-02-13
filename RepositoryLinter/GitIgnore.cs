namespace RepositoryLinter;

public class GitIgnore
{
    private readonly Ignore.Ignore _ignore = new();
    
    /// <summary>
    /// GitIgnore constructor. If gitignore is disabled, this will not read the .gitignore file.
    /// </summary>
    /// <param name="pathToGitRepo">Path to a Git repository.</param>
    /// <param name="enabled">When enabled, the .gitignore file is read and parsed.</param>
    public GitIgnore(string pathToGitRepo, bool enabled = true)
    {
        var gitignore = Path.Join(pathToGitRepo, ".gitignore");
        if (!enabled || !File.Exists(gitignore))
        {
            return;
        }
        
        var lines = File.ReadAllLines(gitignore);
        
        // Filter out comments and empty lines
        lines = lines.Where(x => !x.StartsWith('#') && !string.IsNullOrWhiteSpace(x)).ToArray();
        
        // Add the lines to the ignore object
        foreach (var line in lines)
        {
            _ignore.Add(line);
        }
    }
    
    /// <summary>
    /// Check if a file is ignored by the .gitignore file. If gitignore is disabled, this will always return false.
    /// </summary>
    /// <param name="file">Filepath to check</param>
    /// <returns>True if ignored, false otherwise or if gitignore is disabled</returns>
    public bool IsIgnored(string file)
    {
        var ignored = _ignore.IsIgnored(file);
        return ignored;
    }
}