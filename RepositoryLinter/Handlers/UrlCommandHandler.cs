using System.CommandLine.Parsing;

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
        
            // Run linting pipeline
            var code = runner.Run(git);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to lint: {url}");
            Console.Error.WriteLine(e.Message);
            Environment.Exit(1);
        }
        
        return Task.CompletedTask;
    }
    
    public static void Validate(ArgumentResult url)
    {
        if (!Uri.TryCreate(url.Tokens[0].Value, UriKind.Absolute, out _))
        {
            url.ErrorMessage = $"Invalid URL: {url.Tokens[0].Value}";
        }
    }
}