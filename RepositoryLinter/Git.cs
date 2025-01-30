using System.Diagnostics;

namespace RepositoryLinter;

public class Git
{
    public string ParentDirectory { get; }
    public bool SaveToDisk { get; set; } = false;
    private readonly Uri _url = null!;
    // Get name of the repository
    public readonly string RepositoryName;
    public string PathToGitDirectory { get; }
    
    /// <summary>
    /// Creates a new Git object with a URL
    /// </summary>
    /// <param name="url">Url to a Git repository</param>
    /// <param name="parentDirectory">File system path to a directory to where the repository is to be cloned</param>
    public Git(Uri url, string parentDirectory = "/tmp/repolinter/git")
    {
        if (!Directory.Exists(parentDirectory))
        {
            Directory.CreateDirectory(parentDirectory);
        }
           
        ParentDirectory = parentDirectory;
        _url = url;
        RepositoryName = Path.GetFileName(_url.LocalPath);
        PathToGitDirectory = Path.Join(ParentDirectory, RepositoryName);
    }

    public Git(string localPath)
    {
        if (!Directory.Exists(localPath))
        {
            throw new DirectoryNotFoundException("Path does not exist");
        }
        
        // Check if the path is a git repository
        if (!Directory.Exists(Path.Join(localPath, ".git")))
        {
            throw new Exception("Path is not a git repository");
        }
        
        // Split the path to get the directory and the repository name
        RepositoryName = Path.GetFileName(localPath);
        ParentDirectory = Path.GetDirectoryName(localPath)!;
        PathToGitDirectory = localPath;
        SaveToDisk = true;
    }
    
    ~Git()
    {
        if (!SaveToDisk)
        {
            DeleteGitDirectory();
        }
    }
    
    /// <summary>
    /// Removes the cloned git repository
    /// </summary>
    private void DeleteGitDirectory()
    {
        Directory.Delete(PathToGitDirectory, true);
    }

    public void Clone()
    {
        // Check if the repository already exists, if so, delete it
        if (Directory.Exists(PathToGitDirectory))
        {
            DeleteGitDirectory();
        }
        
        // Clone the repository using git command
        var p = new Process
        {
            StartInfo =
            {
                FileName = "git",
                Arguments = $"clone {_url} {PathToGitDirectory}",
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
    
    public int GetCommitCount()
    {
        var p = new Process
        {
            StartInfo =
            {
                FileName = "git",
                Arguments = $"rev-list --count HEAD",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = PathToGitDirectory
            }
        };
        
        var started = p.Start();
        if (!started)
        {
            throw new Exception("Failed to start git rev-list");
        }
        
        var output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        
        return int.Parse(output);
    }
    
    public IEnumerable<string> GetContributors()
    {
        var p = new Process
        {
            StartInfo =
            {
                FileName = "git",
                Arguments = $"shortlog -sne HEAD",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = PathToGitDirectory
            }
        };
        
        var started = p.Start();
        if (!started)
        {
            throw new Exception("Failed to start git shortlog");
        }
        
        var output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        try
        {
            var lines = output.Split("\n");
        
            return lines.Select(line =>
            {
                var parts = line.Split("\t");
                return parts[1];
            });
        }
        // If the output is empty, return an empty list
        catch (IndexOutOfRangeException)
        {
            return new List<string>();
        }
    }
}