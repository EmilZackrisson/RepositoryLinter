using RepositoryLinter.Checks;
using RepositoryLinter.Exceptions;

namespace RepositoryLinter;

public class LintRunner(GlobalConfiguration config)
{
    /// <summary>
    /// Run the linting pipeline
    /// </summary>
    /// <returns>0 if all checks passes, 1 otherwise</returns>
    /// <param name="git">Git object to run linting pipeline on</param>
    public int Run(Git git)
    {
        // Run linting pipeline
        var linter = new Linter(git, config);

        linter.AddCheck(new FileExistsCheck(".gitignore", git.PathToGitDirectory)
        {
            Name = ".gitignore exists",
            Description = "Check if .gitignore exists",
            TipToFix =
                "Create a .gitignore file. Read more about .gitignore files at https://docs.github.com/en/get-started/getting-started-with-git/ignoring-files"
        });

        linter.AddCheck(new FileExistsCheck("README.*", git.PathToGitDirectory)
        {
            Name = "README exists",
            Description = "Check if README exists",
            TipToFix = "Create a README file.",
            StatusWhenEmpty = CheckStatus.Yellow
        });

        linter.AddCheck(new LicenseFileChecker(git.PathToGitDirectory)
        {
            Name = "License Exists",
            Description = "Check if a LICENSE exists in the repository.",
            TipToFix =
                "Create a LICENSE file. Read more about licenses at https://choosealicense.com/ and https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository"
        });

        linter.AddCheck(new DirectoryExistsCheck(".github/workflows", git.PathToGitDirectory)
        {
            Name = "GitHub Workflow exists",
            Description = "Check if GitHub Workflow exists",
            TipToFix =
                "Create a GitHub Workflow. Read more about GitHub workflows at https://docs.github.com/en/actions/learn-github-actions",
            StatusWhenFailed = CheckStatus.Red,
            StatusWhenEmpty = CheckStatus.Yellow,
            ShouldContainFiles = ["*.yaml", "*.yml"]
        });

        linter.AddCheck(new SecretsCheck(git.PathToGitDirectory, config)
        {
            Name = "Secrets check",
            Description = "Check if the repository contains any secrets",
            TipToFix = "Remove the secrets found",
            StatusWhenFailed = CheckStatus.Red
        });

        linter.AddCheck(new FilePathContainsStringChecker("test", git.PathToGitDirectory)
        {
            Name = "Tests exists",
            Description = "Check if the file path contains the string 'test'",
            TipToFix = "Add tests",
            StatusWhenFound = CheckStatus.Green,
            StatusWhenNotFound = CheckStatus.Red
        });

        linter.Run();

        linter.ChangeAllowedToFail();

        linter.PrintResults();

        if (linter.StatusCode() != 0)
        {
            throw new CheckFailedException("One or more checks failed. Please fix the issues and try again.");
        }

        return linter.StatusCode();
    }
}