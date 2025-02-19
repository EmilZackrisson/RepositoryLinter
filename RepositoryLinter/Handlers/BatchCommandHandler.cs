using System.CommandLine.Parsing;
using RepositoryLinter.Exceptions;

namespace RepositoryLinter.Handlers;

public class BatchCommandHandler(LintRunner runner, GlobalConfiguration config)
{
    public Task Handle(FileInfo repos)
    {
        Console.WriteLine($"Linting batch file: {repos}");
        var lines = File.ReadAllLines(repos.FullName);
        
        // Filter out empty lines and comments
        lines = lines.Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith('#')).ToArray();
        
        var hasErrors = false;
        
        foreach (var line in lines)
        {
            try
            {
                var isUrl = Uri.TryCreate(line, UriKind.Absolute, out _);
                if (isUrl)
                {
                    var urlHandler = new UrlCommandHandler(runner, config);
                    urlHandler.Handle(line, config.PathToSaveGitRepos);
                }
                else
                {
                    var pathHandler = new PathCommandHandler(runner);
                    pathHandler.Handle(line);
                }

                // Print a line of = to separate each lint if not the last one
                if (line != lines[^1])
                {
                    Console.WriteLine(new string('=', Console.WindowWidth));
                }
            }
            catch (CheckFailedException)
            {
                hasErrors = true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }
        
        if (hasErrors)
        {
            Environment.Exit(100);
        }
        
        return Task.CompletedTask;
    }
    
    public static void Validate(ArgumentResult repo)
    {
        var file = repo.Tokens[0].Value;
        if (!File.Exists(file))
        {
            repo.ErrorMessage = $"File does not exist: {file}";
        }
        else
        {
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                var isUrl = Uri.TryCreate(line, UriKind.Absolute, out _);
                if (isUrl || Directory.Exists(line)) continue;
                repo.ErrorMessage = $"Invalid path or URL: {line}";
                break;
            }
        }
    }
}