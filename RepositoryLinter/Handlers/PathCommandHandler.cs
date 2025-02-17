using System.CommandLine.Parsing;
using RepositoryLinter.Exceptions;

namespace RepositoryLinter.Handlers;

public class PathCommandHandler(LintRunner runner)
{
    public Task Handle(string path)
    {
        path = path.TrimEnd('/');
        Console.WriteLine($"Linting path: {path}");

        try
        {
            var git = new Git(path);
            runner.Run(git);
        }
        catch (CheckFailedException)
        {
            Environment.Exit(100);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to lint: {path}", e);
            Environment.Exit(1);
        }
        
        return Task.CompletedTask;
    }
    
    public static void Validate(ArgumentResult path)
    {
        if (!Directory.Exists(path.Tokens[0].Value))
        {
            path.ErrorMessage = $"Invalid path: {path.Tokens[0].Value}";
        }
    }
}