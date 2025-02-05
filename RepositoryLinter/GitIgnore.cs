namespace RepositoryLinter;

public class GitIgnore
{
    private Ignore.Ignore _ignore = new();
    private bool _enabled;
    
    public GitIgnore(string pathToGitRepo, bool enabled = true)
    {
        if (!enabled)
        {
            Console.WriteLine("GitIgnore is disabled");
            return;
        }
        
        _enabled = enabled;
        var gitignore = Path.Join(pathToGitRepo, ".gitignore");
        
        var lines = File.ReadAllLines(gitignore);
            
        foreach (var line in lines)
        {
            Console.WriteLine($"Adding {line} to ignore list");
            _ignore.Add(line);
        }
    }
    
    public bool IsIgnored(string file)
    {
        var ignored = _ignore.IsIgnored(file);
        //Console.WriteLine($"File {file} is ignored: {ignored}");
        return ignored;
    }
}