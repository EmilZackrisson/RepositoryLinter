using RepositoryLinter;
using Xunit.Abstractions;

namespace RepoLinterTests;

public class GitTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public GitTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }


    [Fact]
    public void Clone()
    {
        Git git = new Git(new Uri("https://github.com/EmilZackrisson/TraefikAPI"), "/tmp/repolinter/git");
        git.Clone();
        var directory = Path.Join(git.PathToGitDirectory, git.RepositoryName);
        _testOutputHelper.WriteLine("Git repository cloned to: " + directory);
        Assert.True(Directory.Exists(directory));
    }

    [Fact]
    public void GitCleanup_IfNotDisabled()
    {
        Git git = new Git(new Uri("https://github.com/EmilZackrisson/TraefikAPI"), "/tmp/repolinter/git");
        var directory = git.PathToGitDirectory;
        git.SaveToDisk = false;
        git.Clone();
        Assert.True(Directory.Exists(directory));
        git = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        Assert.False(Directory.Exists(directory));
    }
}