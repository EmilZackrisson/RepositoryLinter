using System.Diagnostics;
using RepositoryLinter.Exceptions;

namespace RepositoryLinter;

public class Git
{
    /// <summary>
    /// Path to the parent directory where the Git repository is saved
    /// </summary>
    public string ParentDirectory { get; }

    private readonly Uri _url = null!;

    /// <summary>
    /// The name of the repository. This is the last part of the URL.
    /// </summary>
    public readonly string RepositoryName;

    /// <summary>
    /// Path to the cloned Git repository
    /// </summary>
    public string PathToGitDirectory { get; }

    /// <summary>
    /// Flag to determine if the Git repository should be deleted when the object is destroyed
    /// </summary>
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
        try
        {
            Directory.Delete(PathToGitDirectory, true);
        }
        catch (DirectoryNotFoundException)
        {
            // The directory does not exist, do nothing.
        }
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

        // Clone the repository in the parent directory
        var p = CreateAndStartGitProcess($"clone {_url} {RepositoryName}", ParentDirectory);

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
        var p = CreateAndStartGitProcess("rev-list --count HEAD");

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
        var p = CreateAndStartGitProcess("shortlog -sne HEAD");

        var output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        var lines = output.Split("\n");

        return (from line in lines select line.Trim() into tmp where tmp.Length != 0 select tmp.Split("\t")[1].Trim())
            .ToList();
    }

    public List<string> GetContributorsWithCommits()
    {
        var p = CreateAndStartGitProcess("shortlog -sne HEAD");

        var output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        var lines = output.Split("\n");

        return lines.Select(line => line.Trim()).Where(tmp => tmp.Length != 0).ToList();
    }

    /// <summary>
    /// Creates a new process for running git commands.
    /// </summary>
    /// <param name="arguments">Arguments to start git process with</param>
    /// <param name="workingDirectory">Working directory for process</param>
    /// <returns>A git process</returns>
    /// <exception cref="ProcessFailedToStartException">Git process failed to start</exception>
    private Process CreateAndStartGitProcess(string arguments, string? workingDirectory = null)
    {
        var p = new Process
        {
            StartInfo =
            {
                FileName = "git",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? PathToGitDirectory
            }
        };

        var started = p.Start();
        if (!started) throw new ProcessFailedToStartException("Failed to start git command");

        return p;
    }
}