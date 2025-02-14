namespace RepositoryLinter.Checks;

public class FilePathContainsStringChecker(string stringToFind, string pathToGitRepo, GlobalConfiguration config) : Checker
{
    private readonly List<string> _foundPaths = [];
    public override void Run()
    {
        var paths = GetAllFilePaths().ToList();
        
        foreach (var path in paths)
        {
            var found = path.Contains(stringToFind);

            if (!found) continue;
            
            _foundPaths.Add(path);
            Status = CheckStatus.Red;
        }
    }

    private IEnumerable<string> GetAllFilePaths()
    {
        return Directory.EnumerateFiles(pathToGitRepo, "*.*", SearchOption.AllDirectories);
    }

    public override string ToString()
    {
        if (Status == CheckStatus.Green)
        {
            return base.ToString();
        }
        
        return base.ToString() + "\n" + "Found string in paths:" + "\n" + string.Join("\n", _foundPaths);
    }
}