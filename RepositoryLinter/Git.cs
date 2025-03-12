using System.Diagnostics;
using RepositoryLinter.Exceptions;

namespace RepositoryLinter;

public class Git
{
    public string ParentDirectory { get; }
    private readonly Uri _url = null!;
    public readonly string RepositoryName;
    public string PathToGitDirectory { get; }
    private readonly bool _cleanup;

    /// <summary>
    /// Creates a new Git object with a URL
    /// </summary>
    /// <param name="url">Url to a Git repository</param>
    /// <param name="config">A GlobalConfiguration object</param>
    public Git(Uri url, GlobalConfiguration config)
    {
        if (!Directory.Exists(config.PathToSaveGitRepos))
        {
            Directory.CreateDirectory(config.PathToSaveGitRepos);
        }

        ParentDirectory = config.PathToSaveGitRepos;
        _url = url;

        // Get repo name
        RepositoryName = url.ToString().TrimEnd('/').Split('/')[^1];
        PathToGitDirectory = Path.Join(ParentDirectory, RepositoryName);
        _cleanup = !config.CleanUp;

        if (Directory.Exists(PathToGitDirectory))
        {
            DeleteGitDirectory();
        }
    }

    /// <summary>
    /// Creates a new Git object with a local path
    /// </summary>
    /// <param name="localPath">A local path to a Git repository</param>
    /// <exception cref="DirectoryNotFoundException">Throws if the specified directory does not exist.</exception>
    /// <exception cref="Exception">Throws if the specified directory is not a Git repository.</exception>
    public Git(string localPath)
    {
        if (!Directory.Exists(localPath))
        {
            throw new DirectoryNotFoundException("Path does not exist");
        }


        // Check if the path is a git repository
        if (!Directory.Exists(Path.Join(localPath, ".git")))
        {
            throw new GitException("Path is not a git repository");
        }

        // Split the path to get the directory and the repository name
        RepositoryName = Path.GetFileName(localPath);
        ParentDirectory = Path.GetDirectoryName(localPath)!;
        PathToGitDirectory = localPath;
        _cleanup = true;
    }

    ~Git()
    {
        if (_cleanup)
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

    /// <summary>
    /// Clones the Git repository to the specified directory.
    /// </summary>
    /// <exception cref="Exception">Thrown when the git clone process fails to start or the clone operation fails.</exception>
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
            throw new ProcessFailedToStartException("Failed to start git clone");
        }

        p.WaitForExit();

        if (p.ExitCode != 0)
        {
            throw new GitException("Failed to clone git repository");
        }
    }

    /// <summary>
    /// Gets the commit count of the Git repository.
    /// </summary>
    /// <returns>The number of commits in the repository.</returns>
    /// <exception cref="Exception">Thrown when the git rev-list process fails to start.</exception>
    public int? GetCommitCount()
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
            throw new GitException("Failed to start git rev-list");
        }

        var output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        if (p.ExitCode != 0)
        {
            return null;
        }

        return int.Parse(output);
    }

    /// <summary>
    /// Returns a list of contributors to the repository
    /// </summary>
    /// <returns>A list of contributors</returns>
    /// <exception cref="Exception">Failed to run git command</exception>
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
            throw new GitException("Failed to start git shortlog");
        }

        var output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        var lines = output.Split("\n");
        var contributors = new List<string>();

        foreach (var line in lines)
        {
            var tmp = line.Trim();
            if (tmp.Length == 0)
            {
                continue;
            }

            contributors.Add(tmp.Split("\t")[1].Trim());
        }

        return contributors;
    }

    public List<string> GetContributorsWithCommits()
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
            throw new GitException("Failed to start git shortlog");
        }

        var output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        var lines = output.Split("\n");
        var contributors = new List<string>();

        foreach (var line in lines)
        {
            var tmp = line.Trim();
            if (tmp.Length == 0)
            {
                continue;
            }

            contributors.Add(tmp);
        }

        return contributors;
    }
}