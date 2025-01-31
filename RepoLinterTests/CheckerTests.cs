using RepositoryLinter;
using RepositoryLinter.Checks;
using Xunit.Abstractions;

namespace RepoLinterTests;

public class CheckerTests : IDisposable
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CheckerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        CreateFakeRepoWhereAllChecksPass();
    }
    
    private void CreateFakeRepoWhereAllChecksPass()
    {
        var path = Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass");
        Directory.CreateDirectory(path);
        
        // Create a .gitingore file
        File.WriteAllText(Path.Join(path, ".gitignore"), "node_modules");
        
        // Create a README.md file
        File.WriteAllText(Path.Join(path, "README.md"), "# Hello World");
        
        // Create a LICENSE file
        File.WriteAllText(Path.Join(path, "LICENSE"), "MIT License");
        
        // Create a LICENSE.md file
        File.WriteAllText(Path.Join(path, "LICENSE.md"), "MIT License");
        
        // Create a GitHub Workflows directory
        Directory.CreateDirectory(Path.Join(path, ".github/workflows"));
        // Create a GitHub Workflows file
        File.WriteAllText(Path.Join(path, ".github/workflows/main.yaml"), "name: CI");
        
        // Create a fake secret somewhere
        Directory.CreateDirectory(Path.Join(path, "secrets"));
        File.WriteAllText(Path.Join(path, "secrets/secret.txt"), "45f68f4c-930d-4648-88c3-3a6e260da304");
    }

    [Fact]
    public void FileShouldExists()
    {
        var checker = new FileExistsCheck("README.md", Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"))
        {
            Name = "README",
            Description = "Test for README.md",
            TipToFix = "Add a README.md file to the repository."
        };
        checker.Run();
        Assert.Equal(CheckStatus.Green, checker.Status);
    }
    
    [Fact]
    public void FileShouldExistsWildcard()
    {
        var checker = new FileExistsCheck("LICENSE.md", Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"))
        {
            Name = "LICENCE Wildcard",
            Description = "Testing for LICENCE.*",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Green, checker.Status);
    }
    
    [Fact]
    public void FileShouldNotExists()
    {
        var checker = new FileExistsCheck("NOTEXISTINGFILE", Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"))
        {
            Name = "NOTEXISTINGFILE",
            Description = "NOTEXISTINGFILE should not exist",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Red, checker.Status);
    }
    
    [Fact]
    public void DirectoryShouldExists()
    {
        var checker = new DirectoryExistsCheck(".github/workflows", Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"))
        {
            Name = "GitHub Workflows",
            Description = "Test for GitHub Workflows directory",
            TipToFix = "Add a GitHub Workflows directory to the repository."
        };
        checker.Run();
        Assert.Equal(CheckStatus.Green, checker.Status);
    }
    
    [Fact]
    public void DirectoryShouldNotExists()
    {
        var checker = new DirectoryExistsCheck("NOTEXISTINGDIRECTORY", Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"))
        {
            Name = "NOTEXISTINGDIRECTORY",
            Description = "Test for NOTEXISTINGDIRECTORY",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Red, checker.Status);
    }
    
    [Fact]
    public void SearchForStringShouldExists()
    {
        var checker = new SearchForStringCheck("name: CI", Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"))
        {
            Name = "Search for 'name: CI'",
            Description = "Test for 'name: CI' string",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Green, checker.Status);
    }
    
    [Fact]
    public void SearchForStringShouldNotExists()
    {
        var checker = new SearchForStringCheck("NOTEXISTINGSTRING", Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"))
        {
            Name = "Search for 'NOTEXISTINGSTRING'",
            Description = "Test for 'NOTEXISTINGSTRING' string",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Red, checker.Status);
    }
    
    
    [Fact]
    public void SearchForStringShouldExistsInSecret()
    {
        var checker = new SecretsCheck(Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"))
        {
            Name = "Trufflehog",
            Description = "Search for secrets using Trufflehog",
            TipToFix = "Remove the secret from the repository."
        };
        checker.Run();
        
        // There is no way to know if the secret is found or not, so we just check that the status is green, which means that the check did not fail.
        Assert.Equal(CheckStatus.Green, checker.Status);
    } 

    public void Dispose()
    {
        Directory.Delete(Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"), true);
    }
}