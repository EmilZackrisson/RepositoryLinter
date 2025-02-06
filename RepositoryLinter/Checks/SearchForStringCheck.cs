namespace RepositoryLinter.Checks;

/// <summary>
/// Search for a string in the repository, and set the status based on the result. By default, the status is green if the string is found. Set InvertResult to true to change this behavior.
/// </summary>
/// <param name="searchString">String to search for</param>
/// <param name="gitRepoPath">Path to git repository</param>
public class SearchForStringCheck(string searchString, string gitRepoPath, GlobalConfiguration config) : Checker
{
    public bool InvertResult { get; init; } = false;
    private readonly List<string> _files = [];
    private string _additionalInfo = "";
    public override void Run()
    {
        var files = Directory.EnumerateFiles(gitRepoPath, "*.*", SearchOption.AllDirectories);
        var gitIgnore = new GitIgnore(gitRepoPath, config.GitIgnoreEnabled);

        foreach (var file in files)
        {
            if (gitIgnore.IsIgnored(file))
            {
                _additionalInfo = "Found string in files that are ignored by .gitignore. If you want to search in these files, run the program with the --ignore-gitignore flag.";
                continue;
            }
            
            // Read the file in chunks to avoid reading the entire file into memory
            const int chunkSize = 1024;
            using var fileReader = File.OpenText(file);
            int bytesRead;
            var buffer = new char[chunkSize];
            while ((bytesRead = fileReader.Read(buffer, 0, chunkSize)) > 0)
            {
                var chunk = new string(buffer, 0, bytesRead);
                
                if (!chunk.Contains(searchString)) continue;
                
                _files.Add(file);
                break;
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
        return base.ToString() + $"\nSearch string: {searchString} found in following files:\n{string.Join("\n", _files)}\n{_additionalInfo}";
    }
}