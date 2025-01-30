namespace RepositoryLinter.Checks;

/// <summary>
/// Search for a string in the repository, and set the status based on the result. By default, the status is green if the string is found. Set InvertResult to true to change this behavior.
/// </summary>
/// <param name="searchString">String to search for</param>
/// <param name="gitRepoPath">Path to git repository</param>
public class SearchForStringCheck(string searchString, string gitRepoPath) : Checker
{
    public bool InvertResult { get; init; } = false;
    private readonly List<string> _files = [];
    public override void Run()
    {
        var files = Directory.EnumerateFiles(gitRepoPath, "*.*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            if (File.ReadAllText(file).Contains(searchString))
            {
                _files.Add(file);
            }
        }

        var found = _files.Count != 0;
        if (InvertResult)
        {
            found = !found;
        }
        Status = found ? CheckStatus.Green : StatusWhenFailed;
    }

    public override string ToString()
    {
        return base.ToString() + $"\nSearch string: {searchString} found in following files:\n{string.Join("\n", _files)}";
    }
}