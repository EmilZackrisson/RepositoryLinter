namespace RepositoryLinter.Checks;

public class FilePathContainsStringChecker(string stringToFind, string pathToGitRepo) : Checker
{
    private readonly List<string> _foundPaths = [];
    public override void Run()
    {
        var paths = GetAllFilePaths().ToList();
        
        foreach (var path in paths)
        {
            var found = path.Contains(stringToFind);

            if (found)
            {
                _foundPaths.Add(path);
            }
        }
        
        Status = _foundPaths.Count == 0 ? CheckStatus.Green : CheckStatus.Red;
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