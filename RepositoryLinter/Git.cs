using System.Diagnostics;

namespace RepositoryLinter;

public class Git
{
    public string PathToGitDirectory { get; }
    public bool SaveToDisk { get; set; } = false;
    private readonly Uri _url;
    // Get name of the repository
    public readonly string RepositoryName;
    
    /// <summary>
    /// Creates a new Git object with a URL
    /// </summary>
    /// <param name="url">Url to a Git repository</param>
    /// <param name="pathToGitDirectory">File system path to a directory to where the repository is to be cloned</param>
    public Git(Uri url, string pathToGitDirectory = "/tmp/repolinter/git")
    {
        if (!Directory.Exists(pathToGitDirectory))
        {
            throw new ArgumentException($"Directory {PathToGitDirectory} does not exist");
        }
           
        PathToGitDirectory = pathToGitDirectory;
        _url = url;
        RepositoryName = Path.GetFileName(_url.LocalPath);
    }
    
    ~Git()
    {
        if (SaveToDisk) return;
        try
        {
            Directory.Delete(Path.Join(PathToGitDirectory, RepositoryName), true);
            Console.WriteLine("Deleted git repository");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void Clone()
    {
        
        // Clone the repository using git command
        var p = new Process
        {
            StartInfo =
            {
                FileName = "git",
                Arguments = $"clone {_url} {Path.Join(PathToGitDirectory, RepositoryName)}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        var started = p.Start();
        if (!started)
        {
            throw new Exception("Failed to start git clone");
        }
        
        p.WaitForExit();
    }
    
}