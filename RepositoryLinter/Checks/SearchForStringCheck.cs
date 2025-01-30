namespace RepositoryLinter.Checks;

/// <summary>
/// Search for a string in the repository, and set the status based on the result. By default, the status is green if the string is found. Set InvertResult to true to change this behavior.
/// </summary>
/// <param name="searchString">String to search for</param>
/// <param name="gitRepoPath">Path to git repository</param>
public class SearchForStringCheck(string searchString, string gitRepoPath) : Checker
{
    public bool InvertResult { get; set; } = false;
    public override void Run()
    {
        var files = Directory.EnumerateFiles(gitRepoPath, "*.*", SearchOption.AllDirectories);
        var found = files.Any(file => File.ReadAllText(file).Contains(searchString));
        if (InvertResult)
        {
            found = !found;
        }
        Status = found ? CheckStatus.Green : StatusWhenFailed;
    }
}