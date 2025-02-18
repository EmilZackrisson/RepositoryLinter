using RepositoryLinter;
using RepositoryLinter.Checks;
using Xunit.Abstractions;

namespace RepoLinterTests;

public class CheckerTests : IDisposable
{
    private readonly GlobalConfiguration _config = new(); 
    public CheckerTests()
    {
        CreateFakeRepoWhereAllChecksPass();
    }
    
    private void CreateFakeRepoWhereAllChecksPass()
    {
        // Create a fake repository where all checks pass
        var path = Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass");
        Directory.CreateDirectory(path);
        
        // Create a .gitingore file
        File.WriteAllText(Path.Join(path, ".gitignore"), "node_modules");
        
        // Create a README.md file
        File.WriteAllText(Path.Join(path, "README.md"), "# Hello World");
        
        // Create a LICENSE file
        File.WriteAllText(Path.Join(path, "LICENSE"), "MIT LICENSE");
        
        // Create a LICENSE.md file
        File.WriteAllText(Path.Join(path, "LICENSE.md"), "MIT LICENSE");
        
        // Create a LICENSE_Empty file
        File.WriteAllText(Path.Join(path, "LICENSE_Empty"), "");
        
        // Create a LICENSE directory with a LICENSE file
        Directory.CreateDirectory(Path.Join(path, "license_dir"));
        File.WriteAllText(Path.Join(path, "license_dir/LICENSE.indir"), "MIT LICENSE");
        
        // Create a GitHub Workflows directory
        Directory.CreateDirectory(Path.Join(path, ".github/workflows"));
        // Create a GitHub Workflows file
        File.WriteAllText(Path.Join(path, ".github/workflows/main.yaml"), "name: CI");
        
        // Create a fake secret somewhere
        Directory.CreateDirectory(Path.Join(path, "secrets"));
        File.WriteAllText(Path.Join(path, "secrets/secret.txt"), "45f68f4c-930d-4648-88c3-3a6e260da304");
        
        // Create a directory without any secrets
        Directory.CreateDirectory(Path.Join(path, "no-secrets"));
        
        // Create a directory without any license
        Directory.CreateDirectory(Path.Join(path, "no-license"));
        
        // Create a directory with a long name that should be found
        Directory.CreateDirectory(Path.Join(path, "thisisalongdirectorynamethatisnotrandomandshouldbefound"));
    }
    
    public void Dispose()
    {
        Directory.Delete(Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"), true);
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
        var checker = new FileExistsCheck("LICENSE.*", Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"))
        {
            Name = "LICENSE Wildcard",
            Description = "Testing for LICENSE.*",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Yellow, checker.Status); // Yellow because there are multiple files matching the wildcard
    }
    
    [Fact]
    public void FileShouldNotBeEmpty()
    {
        var checker = new FileExistsCheck("LICENSE.md", Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"))
        {
            Name = "LICENSE Not Empty",
            Description = "Testing for LICENSE.md to not be empty",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Green, checker.Status);
    }
    
    [Fact]
    public void FileShouldBeEmpty()
    {
        var checker = new FileExistsCheck("LICENSE_Empty", Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"))
        {
            Name = "LICENSE Empty",
            Description = "Testing for LICENSE to be empty",
            TipToFix = "",
            StatusWhenEmpty = CheckStatus.Yellow
        };
        checker.Run();
        Assert.Equal(CheckStatus.Yellow, checker.Status);
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
        var checker = new SearchForStringCheck("name: CI", Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"), _config)
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
        var checker = new SearchForStringCheck("NOTEXISTINGSTRING", Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"), _config)
        {
            Name = "Search for 'NOTEXISTINGSTRING'",
            Description = "Test for 'NOTEXISTINGSTRING' string",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Red, checker.Status);
    }
    
    [Fact]
    public void SecretsShouldNotExists()
    {
        var checker = new SecretsCheck(Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass", "no-secrets"), _config)
        {
            Name = "Secrets",
            Description = "Test for secrets",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Green, checker.Status);
    }
    
    [Fact]
    public void LicenseFileCheckerShouldExists()
    {
        var checker = new LicenseFileChecker(Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"))
        {
            Name = "License File Checker",
            Description = "Test for LICENSE file",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Green, checker.Status);
    }
    
    [Fact]
    public void LicenseFileCheckerShouldNotExists()
    {
        var checker = new LicenseFileChecker(Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass", "no-license"))
        {
            Name = "License File Checker",
            Description = "Test for LICENSE file",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Red, checker.Status);
    }
    
    [Fact]
    public void LicenseFileInDirectoryCheckerShouldExists()
    {
        var checker = new LicenseFileChecker(Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"))
        {
            Name = "License File In Directory Checker",
            Description = "Test for LICENSE file in LICENSE directory",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Green, checker.Status);
    }
    
    [Fact]
    public void FilePathContainsStringCheckerShouldExists()
    {
        var checker = new FilePathContainsStringChecker("thisisalongdirectorynamethatisnotrandomandshouldbefound", Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"), _config)
        {
            Name = "File Path Contains String",
            Description = "Test for file path containing long string",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Green, checker.Status);
    }
    [Fact]
    public void FilePathContainsStringCheckerShouldNotExists()
    {
        var checker = new FilePathContainsStringChecker("thisisalongdirectorynamethatisnotrandomandshouldbenotfound", Path.Join(Directory.GetCurrentDirectory(), "FakeRepoWhereAllChecksPass"), _config)
        {
            Name = "File Path Contains String",
            Description = "Test for file path containing long string",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Green, checker.Status);
    }
}