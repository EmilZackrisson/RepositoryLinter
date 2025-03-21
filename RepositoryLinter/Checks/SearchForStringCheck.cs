namespace RepositoryLinter.Checks;

/// <summary>
/// Search for a string in the repository, and set the status based on the result. By default, the status is green if the string is found. Set InvertResult to true to change this behavior.
/// </summary>
/// <param name="searchString">String to search for</param>
/// <param name="gitRepoPath">Path to git repository</param>
public class SearchForStringCheck(string searchString, string gitRepoPath, GlobalConfiguration config) : Checker
{
    /// <summary>
    /// Invert the result of the check. Default is false.
    /// </summary>
    public bool InvertResult { get; init; }

    private readonly List<string> _files = [];

    public override void Run()
    {
        var files = Directory.EnumerateFiles(gitRepoPath, "*.*", SearchOption.AllDirectories);
        var gitIgnore = new GitCheckIgnore(gitRepoPath, config.GitIgnoreEnabled);
        var hasIgnoredFiles = false;

        foreach (var file in files)
        {
            var fileIsIgnored = gitIgnore.IsIgnored(file);
            if (fileIsIgnored && hasIgnoredFiles)
            {
                continue; // Skip files that are ignored by .gitignore if we have already found a file that is ignored.
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

                if (fileIsIgnored)
                {
                    AdditionalInformation =
                        "Found string in files that are ignored by .gitignore. If you want to search in these files, run the program with the --ignore-gitignore flag.";
                    hasIgnoredFiles = true;
                    break;
                }

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
        if (Status == CheckStatus.Green)
        {
            return base.ToString();
        }

        return base.ToString() +
               $"\nSearch string: {searchString} found in following files:\n{string.Join("\n", _files)}\nTotal number of files: {_files.Count}\n{AdditionalInformation}";
    }
}