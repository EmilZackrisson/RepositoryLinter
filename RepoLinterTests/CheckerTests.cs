using RepositoryLinter;
using RepositoryLinter.Checks;

namespace RepoLinterTests;

public class CheckerTests
{
    private readonly GlobalConfiguration _config = new();
    private string _repoPath = "";

    public CheckerTests()
    {
        CreateFakeRepoWhereAllChecksPass();
    }

    private void CreateFakeRepoWhereAllChecksPass()
    {
        // Create a fake repository where all checks pass
        _repoPath = Path.Join(Path.GetTempPath(), "FakeRepoWhereAllTestsPass");
        Directory.CreateDirectory(_repoPath);

        // Create a .gitingore file
        File.WriteAllText(Path.Join(_repoPath, ".gitignore"), "node_modules");

        // Create a README.md file
        File.WriteAllText(Path.Join(_repoPath, "README.md"), "# Hello World");

        // Create a README.txt file
        File.WriteAllText(Path.Join(_repoPath, "README.txt"), "Hello World");

        // Create a LICENSE file
        File.WriteAllText(Path.Join(_repoPath, "LICENSE"), "MIT LICENSE");

        // Create a LICENSE_Empty file
        File.WriteAllText(Path.Join(_repoPath, "EMPTY_LICENSE"), "");

        // Create a GitHub Workflows directory
        Directory.CreateDirectory(Path.Join(_repoPath, ".github/workflows"));
        // Create a GitHub Workflows file
        File.WriteAllText(Path.Join(_repoPath, ".github/workflows/main.yaml"), "name: CI");

        // Create a fake secret somewhere
        Directory.CreateDirectory(Path.Join(_repoPath, "secrets"));
        File.WriteAllText(Path.Join(_repoPath, "secrets/secret.txt"), "45f68f4c-930d-4648-88c3-3a6e260da304");

        // Create a directory without any secrets
        Directory.CreateDirectory(Path.Join(_repoPath, "no-secrets"));

        // Create a directory without any license
        Directory.CreateDirectory(Path.Join(_repoPath, "no-license"));

        // Create a directory with a long name that should be found
        var x = Path.Join(_repoPath, "thisisalongdirectorynamethatisnotrandomandshouldbefound");
        Directory.CreateDirectory(x);
        File.WriteAllText(Path.Join(x, "test"), "HELLO");
    }

    ~CheckerTests()
    {
        Directory.Delete(_repoPath, true);
    }

    [Fact]
    public void FileShouldExists()
    {
        var checker = new FileExistsCheck("README.md", _repoPath)
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
        var checker = new FileExistsCheck("README.*", _repoPath)
        {
            Name = "README Wildcard",
            Description = "Testing for README.*",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Yellow,
            checker.Status); // Yellow because there are multiple files matching the wildcard
    }

    [Fact]
    public void FileShouldNotBeEmpty()
    {
        var checker = new FileExistsCheck("README.md", _repoPath)
        {
            Name = "README Not Empty",
            Description = "Testing for README.md to not be empty",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Green, checker.Status);
    }

    [Fact]
    public void FileShouldBeEmpty()
    {
        var checker = new FileExistsCheck("EMPTY_LICENSE", _repoPath)
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
        var checker = new FileExistsCheck("NOTEXISTINGFILE", _repoPath)
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
        var checker = new DirectoryExistsCheck(".github/workflows", _repoPath)
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
        var checker = new DirectoryExistsCheck("NOTEXISTINGDIRECTORY", _repoPath)
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
        var checker = new SearchForStringCheck("name: CI", _repoPath, _config)
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
        var checker = new SearchForStringCheck("NOTEXISTINGSTRING", _repoPath, _config)
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
        var checker =
            new SecretsCheck(Path.Join(_repoPath, "no-secrets"),
                _config)
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
        var checker = new LicenseFileChecker(_repoPath)
        {
            Name = "License File Checker",
            Description = "Test for LICENSE file",
            TipToFix = ""
        };
        checker.Run();
        Assert.Equal(CheckStatus.Green, checker.Status);
    }

    [Fact]
    public void LicenceFileCheckerMultipleFiles()
    {
        File.WriteAllText(Path.Join(_repoPath, "LICENSE.md"), "MIT LICENSE");

        var checker = new LicenseFileChecker(_repoPath)
        {
            Name = "License File Checker",
            Description = "Test for LICENSE file",
            TipToFix = ""
        };

        checker.Run();
        Assert.Equal(CheckStatus.Yellow, checker.Status);

        File.Delete(Path.Join(_repoPath, "LICENSE.md"));
    }

    [Fact]
    public void LicenseFileCheckerShouldNotExists()
    {
        var checker = new LicenseFileChecker(Path.Join(_repoPath, "no-license"))
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
        var checker = new LicenseFileChecker(_repoPath)
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
        var checker =
            new FilePathContainsStringChecker("thisisalongdirectorynamethatisnotrandomandshouldbefound", _repoPath)
            {
                Name = "File Path Contains String",
                Description = "Test for file _repoPath containing long string",
                TipToFix = ""
            };
        checker.Run();
        Assert.Equal(checker.StatusWhenFound, checker.Status);
    }

    [Fact]
    public void FilePathContainsStringCheckerShouldNotExists()
    {
        var checker =
            new FilePathContainsStringChecker("thisisalongdirectorynamethatisnotrandomandshouldbenotfound", _repoPath)
            {
                Name = "File Path Contains String",
                Description = "Test for file _repoPath containing long string",
                TipToFix = ""
            };
        checker.Run();
        Assert.Equal(checker.StatusWhenNotFound, checker.Status);
    }

    [Fact]
    public void DirectoryExistsWithFileSearch()
    {
        var checker = new DirectoryExistsCheck(".github/workflows", _repoPath)
        {
            Name = "GitHub Workflows Exists",
            Description = "GitHub Workflows Exists",
            TipToFix = "Add it",
            ShouldContainFiles = ["*.yml", "*.yaml"]
        };
        checker.Run();
        Assert.Equal(CheckStatus.Green, checker.Status);
    }

    [Fact]
    public void DirectoryExistsWithNoMatchingFileSearch()
    {
        var checker = new DirectoryExistsCheck(".github/workflows", _repoPath)
        {
            Name = "GitHub Workflows Exists",
            Description = "GitHub Workflows Exists",
            TipToFix = "Add it",
            ShouldContainFiles = ["*.png", "*.jpg"]
        };
        checker.Run();
        Assert.Equal(CheckStatus.Red, checker.Status);
    }
}