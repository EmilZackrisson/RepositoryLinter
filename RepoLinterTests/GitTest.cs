using System.Diagnostics;
using RepositoryLinter;
using Xunit.Abstractions;

namespace RepoLinterTests;

public class GitTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public GitTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        InitFakeRepo();
    }

    private void InitFakeRepo()
    {
        var path = Path.Join(Directory.GetCurrentDirectory(), "FakeGitRepoWhereAllChecksPass");
        Directory.CreateDirectory(path);
        
        
        var inited = RunGitCommand(path, "init");
        if (!inited)
        {
            throw new Exception("Failed to init git repository");
        }

        var x = RunGitCommand(path, "config user.email test@example.com");
        var y = RunGitCommand(path, "config user.name Test");
        
        if (!x || !y)
        {
            throw new Exception("Failed to set git user");
        }
        
        // Create a commit
        File.WriteAllText(Path.Join(path, "README.md"), "# Hello World");
        var added = RunGitCommand(path, "add .");
        var committed = RunGitCommand(path, "commit -m Hello");
        
        if (!added || !committed)
        {
            throw new Exception("Failed to create commit");
        }
    }
    
    private bool RunGitCommand(string path, string command)
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
        Git git = new Git(new Uri("https://github.com/EmilZackrisson/TraefikAPI"));
        git.Clone();
        var directory = Path.Join(git.ParentDirectory, git.RepositoryName);
        _testOutputHelper.WriteLine("Git repository cloned to: " + directory);
        Assert.True(Directory.Exists(directory));
    }
    
    [Fact]
    public void CreateGitObjectWithLocalPath()
    {
        Git git = new Git(Path.Join(Directory.GetCurrentDirectory(), "FakeGitRepoWhereAllChecksPass"));
        Assert.Equal(Path.Join(Directory.GetCurrentDirectory(), "FakeGitRepoWhereAllChecksPass"), Path.Join(git.ParentDirectory, git.RepositoryName));
    }
    
    [Fact]
    public void CreateGitObjectWithLocalPathThatDoesNotExist()
    {
        Assert.Throws<DirectoryNotFoundException>(() => new Git("/tmp/repolinter/git/NonExistent"));
    }
    
    [Fact]
    public void CreateGitObjectWithLocalPathThatIsNotAGitRepository()
    {
        Directory.CreateDirectory("/tmp/repolinter/git/NotAGitRepository");
        Assert.Throws<Exception>(() => new Git("/tmp/repolinter/git/NotAGitRepository"));
        Directory.Delete("/tmp/repolinter/git/NotAGitRepository");
    }

    [Fact]
    public void GetCommitCount()
    {
        Git git = new Git("/Users/emizac/RiderProjects/RepositoryLinter/RepoLinterTests/TestRepos/TestCommitCountAndContribs");
        Assert.Equal(1, git.GetCommitCount());
    }
    
    [Fact]
    public void GetContributors()
    {
        Git git = new Git("/Users/emizac/RiderProjects/RepositoryLinter/RepoLinterTests/TestRepos/TestCommitCountAndContribs");
        var contributors = git.GetContributors();
        Assert.Contains("Test <test@example.com>", contributors);
    }
}