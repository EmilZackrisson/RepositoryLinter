namespace RepositoryLinter;

public class GitIgnoreHandler
{
    public List<string> IgnoredPatterns { get; private set; } = new List<string>();

    public GitIgnoreHandler(string repositoryPath, bool enabled = true)
    {
        if (!enabled) return;
        
        if (!Directory.Exists(repositoryPath))
        {
            throw new DirectoryNotFoundException($"The directory '{repositoryPath}' does not exist.");
        }

        ProcessDirectory(repositoryPath);
    }

    private void ProcessDirectory(string directoryPath)
    {
        foreach (string filePath in Directory.GetFiles(directoryPath))
        {
            if (Path.GetFileName(filePath) == ".gitignore")
            {
                ReadGitIgnoreFile(filePath);
            }
        }

        foreach (string subDirectoryPath in Directory.GetDirectories(directoryPath))
        {
            ProcessDirectory(subDirectoryPath);
        }
    }

    private void ReadGitIgnoreFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            // Ignore comments and empty lines
            if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'))
            {
                IgnoredPatterns.Add(line.Trim());
            }
        }
    }

    public bool IsIgnored(string path)
    {
        var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), path);
        return IgnoredPatterns.Any(pattern => IsMatch(relativePath, pattern));
    }

    private static bool IsMatch(string relativePath, string pattern)
    {
        // You may need to implement a pattern matching logic depending on the complexity of gitignore patterns.
        // For now, we do a simple check
        return relativePath.Contains(pattern);
    }
}