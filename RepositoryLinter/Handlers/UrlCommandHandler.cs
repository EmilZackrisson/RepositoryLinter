using RepositoryLinter.Exceptions;

namespace RepositoryLinter.Handlers;

public class UrlCommandHandler(LintRunner runner, GlobalConfiguration config)
{
    public Task Handle(string url, string? pathToSave)
    {
        Console.WriteLine($"Linting URL: {url}");

        config.PathToSaveGitRepos = pathToSave ?? config.PathToSaveGitRepos;
        
        try
        {
            var git = new Git(new Uri(url), config);
            git.Clone();
            runner.Run(git);

            if (!config.CleanUp)
            {
                Console.WriteLine($"Cleanup is disabled: Cloned repository saved at {git.PathToGitDirectory}");
            }
        }
        catch (CheckFailedException)
        {
            Environment.Exit(100);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to lint: {url}");
            Console.Error.WriteLine(e.Message);
            Environment.Exit(1);
        }
        
        return Task.CompletedTask;
    }
}