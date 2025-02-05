namespace RepositoryLinter;

public class GlobalConfiguration
{
    public bool TruncateOutput { get; set; } = true;
    public bool GitIgnoreEnabled { get; set; } = true;
    public bool CleanUp { get; set; } = true; 
    public bool Verbose { get; set; } = false;
    public string PathToSaveGitRepos { get; set; } = "/tmp/repolinter/git";
}