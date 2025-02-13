using RepositoryLinter.Checks;

namespace RepositoryLinter;

public class LintRunner(GlobalConfiguration config)
{
    /// <summary>
    /// Run the linting pipeline
    /// </summary>
    /// <param name="git">Git object to run linting pipeline on</param>
    public void Run(Git git)
    {
        // Run linting pipeline
        var linter = new Linter(git, config);
    
        linter.AddCheck(new FileExistsCheck("README.*", git.PathToGitDirectory)
        {
            Name = "README exists",
            Description = "Check if README exists",
            TipToFix = "Create a README file.",
        });
    
        linter.AddCheck(new FileExistsCheck("LICENSE.*", git.PathToGitDirectory)
        {
            Name = "LICENSE file does exist",
            Description = "Check if LICENSE exists",
            StatusWhenEmpty = CheckStatus.Yellow,
            TipToFix = "Create a LICENSE file. Read more about licenses at https://choosealicense.com/ and https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository",
        });
    
        linter.AddCheck(new DirectoryExistsCheck(".github/workflows", git.PathToGitDirectory)
        {
            Name = "GitHub Workflow directory exists",
            Description = "Check if GitHub Workflow directory exists",
            TipToFix = "Create a GitHub Workflow directory. Read more about GitHub workflows at https://docs.github.com/en/actions/learn-github-actions",
            StatusWhenFailed = CheckStatus.Yellow
        });
    
        linter.AddCheck(new SearchForStringCheck("test", git.PathToGitDirectory, config)
        {
            Name = "Test string does not exists",
            Description = "Check if the string 'test' exists in the repository",
            TipToFix = "Remove the string 'test' from the repository",
            StatusWhenFailed = CheckStatus.Red,
            InvertResult = true
        }); 
    
        linter.AddCheck(new SecretsCheck(git.PathToGitDirectory, config)
        {
            Name = "Secrets check",
            Description = "Check if the repository contains any secrets",
            TipToFix = "Remove the secrets found",
            StatusWhenFailed = CheckStatus.Red
        });
    
        linter.Run();
        linter.PrintResults();
    }
}