namespace RepositoryLinter;

public class GitIgnore
{
    private readonly Ignore.Ignore _ignore = new();
    
    public GitIgnore(string pathToGitRepo, bool enabled = true)
    {
        if (!enabled)
        {
            Console.WriteLine("GitIgnore is disabled.");
            return;
        }
        var gitignore = Path.Join(pathToGitRepo, ".gitignore");
        
        var lines = File.ReadAllLines(gitignore);
            
        foreach (var line in lines)
        {
            _ignore.Add(line);
        }
    }
    
    public bool IsIgnored(string file)
    {
        var ignored = _ignore.IsIgnored(file);
        return ignored;
    }
}