namespace RepositoryLinter;

public class GlobalConfiguration
{
    /// <summary>
    /// Truncate the output of the checks to 10 lines. Default is true.
    /// </summary>
    public bool TruncateOutput { get; set; } = true;
    
    /// <summary>
    /// Enable or disable the .gitignore file. Default is true.
    /// </summary>
    public bool GitIgnoreEnabled { get; set; } = true;
    
    /// <summary>
    /// Clean up cloned repositories after the program has finished. Local repositories are never deleted. Default is true.
    /// </summary>
    public bool CleanUp { get; set; } = true; 
    
    /// <summary>
    /// Path to save cloned Git repositories. Default is /tmp/repolinter/git.
    /// </summary>
    public string PathToSaveGitRepos { get; set; } = "/tmp/repolinter/git";
}