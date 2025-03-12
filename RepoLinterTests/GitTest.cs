using System.Diagnostics;
using RepositoryLinter;
using RepositoryLinter.Exceptions;
using Xunit.Abstractions;

namespace RepoLinterTests;

public class GitTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _path;
    private readonly GlobalConfiguration _config = new();

    public GitTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _path = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());
        InitFakeRepo();
    }

    private void InitFakeRepo()
    {
        Directory.CreateDirectory(_path);

        // Create a .gitingore file and add a file to ignore
        File.WriteAllText(Path.Join(_path, ".gitignore"), "ignored.txt");

        var initialized = RunGitCommand(_path, "init");
        if (!initialized)
        {
            throw new Exception("Failed to init git repository");
        }

        var x = RunGitCommand(_path, "config --local user.email \"test@example.com\"");
        var y = RunGitCommand(_path, "config --local user.name \"Test\"");

        if (!x || !y)
        {
            throw new GitException("Failed to set git user");
        }

        // Create a commit
        File.WriteAllText(Path.Join(_path, "README.md"), "# Hello World");
        RunGitCommand(_path, "add .");
        RunGitCommand(_path, "branch -m main");
        RunGitCommand(_path, "commit -m Hello");
    }

    private static bool RunGitCommand(string path, string command)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = command,
                WorkingDirectory = path,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var started = process.Start();
        if (!started)
        {
            throw new Exception("Failed to start git command");
        }

        process.WaitForExit();
        return process.ExitCode == 0;
    }

    [Fact]
    public void CloneGitRepository()
    {
        var git = new Git(new Uri("https://github.com/githubtraining/hellogitworld"), _config);
        git.Clone();
        var directory = Path.Join(git.ParentDirectory, git.RepositoryName);
        _testOutputHelper.WriteLine("Git repository cloned to: " + directory);
        Assert.True(Directory.Exists(directory));
    }

    [Fact]
    public void CreateGitObjectWithLocalPath()
    {
        Git git = new Git(_path);
        Assert.Equal(_path, Path.Join(git.ParentDirectory, git.RepositoryName));
    }

    [Fact]
    public void CreateGitObjectWithLocalPathThatDoesNotExist()
    {
        var path = Path.Join(_path, Path.GetRandomFileName());
        Assert.Throws<DirectoryNotFoundException>(() => new Git(path));
    }

    [Fact]
    public void CreateGitObjectWithLocalPathThatIsNotAGitRepository()
    {
        var path = Path.Join(_path, Path.GetRandomFileName());
        Directory.CreateDirectory(path);
        Assert.Throws<GitException>(() => new Git(path));
    }

    [Fact]
    public void GetCommitCount()
    {
        var git = new Git(_path);
        Assert.Equal(1, git.GetCommitCount());
    }

    [Fact]
    public void GetContributors()
    {
        var git = new Git(_path);
        var contributors = git.GetContributors();
        Assert.Contains("Test <test@example.com>", contributors);
    }

    [Fact]
    public void IgnoreFile()
    {
        var ignore = new GitCheckIgnore(_path);
        var pathToIgnoredFile = Path.Join(_path, "ignored.txt");
        var ignored = ignore.IsIgnored(pathToIgnoredFile);
        Assert.True(ignored);
    }

    [Fact]
    public void ShouldNotIgnoreFile()
    {
        var ignore = new GitCheckIgnore(_path);
        var pathToNotIgnoredFile = Path.Join(_path, "README.md");
        var ignored = ignore.IsIgnored(pathToNotIgnoredFile);
        Assert.False(ignored);
    }
}