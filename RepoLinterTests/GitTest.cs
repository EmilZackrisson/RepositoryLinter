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
        Git git = new Git("/tmp/repolinter/git/TraefikAPI");
        Assert.Equal("/tmp/repolinter/git/TraefikAPI", Path.Join(git.ParentDirectory, git.RepositoryName));
    }
}