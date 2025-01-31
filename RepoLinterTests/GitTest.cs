using System.Diagnostics;
using RepositoryLinter;
using Xunit.Abstractions;

namespace RepoLinterTests;

public class GitTest : IDisposable
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _path;
    
    public GitTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _path = Path.Join(Directory.GetCurrentDirectory(), "FakeGitRepoWhereAllChecksPass");
        InitFakeRepo();
    }

    

    private void InitFakeRepo()
    {
        Directory.CreateDirectory(_path);
        
        var inited = RunGitCommand(_path, "init");
        if (!inited)
        {
            throw new Exception("Failed to init git repository");
        }

        var x = RunGitCommand(_path, "config --global user.email \"test@example.com\"");
        var y = RunGitCommand(_path, "config --global user.name \"Test\"");
        
        if (!x || !y)
        {
            throw new Exception("Failed to set git user");
        }
        
        // Create a commit
        File.WriteAllText(Path.Join(_path, "README.md"), "# Hello World");
        var added = RunGitCommand(_path, "add .");
        var branched = RunGitCommand(_path, "branch -m main");
        var committed = RunGitCommand(_path, "commit -m Hello");
        
        /*
        if (!added || !committed)
        {
            throw new Exception("Failed to create commit");
        } */
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
        Git git = new Git(_path);
        Assert.Equal(_path, Path.Join(git.ParentDirectory, git.RepositoryName));
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
        Git git = new Git(_path);
        Assert.Equal(1, git.GetCommitCount());
    }
    
    [Fact]
    public void GetContributors()
    {
        Git git = new Git(_path);
        var contributors = git.GetContributors();
        Assert.Contains("Test <test@example.com>", contributors);
    }

    public void Dispose()
    {
        Directory.Delete(Path.Join(Directory.GetCurrentDirectory(), "FakeGitRepoWhereAllChecksPass"), true);
    }
}