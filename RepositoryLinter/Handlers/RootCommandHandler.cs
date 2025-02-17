using System.CommandLine.Parsing;
using RepositoryLinter.Exceptions;

namespace RepositoryLinter.Handlers;

public class RootCommandHandler(LintRunner runner, GlobalConfiguration config)
{
    enum ArgumentType
    {
        Url,
        Path,
        File,
        Unknown
    }
    
    private ArgumentType _argumentType = ArgumentType.Unknown;
    
    public Task Handle(string urlOrPath)
    {
        switch (_argumentType)
        {
            case ArgumentType.Url:
                var urlHandler = new UrlCommandHandler(runner, config);
                return urlHandler.Handle(urlOrPath, null);
            case ArgumentType.Path:
                var pathHandler = new PathCommandHandler(runner);
                return pathHandler.Handle(urlOrPath);
            case ArgumentType.File:
                var fileHandler = new BatchCommandHandler(runner, config);
                var fileInfo = new FileInfo(urlOrPath);
                return fileHandler.Handle(fileInfo);
            case ArgumentType.Unknown:
            default:
                throw new CheckFailedException("Invalid argument");
        }
    }
    
    public void Validate(ArgumentResult urlOrPath)
    {
        var arg = urlOrPath.Tokens[0].Value;
        
        // Check if the argument is a URL
        if (Uri.TryCreate(arg, UriKind.Absolute, out _) && (arg.StartsWith("http://") || arg.StartsWith("https://")))
        {
            _argumentType = ArgumentType.Url;
            return;
        }
        
        // Check if the argument is a directory
        if (Directory.Exists(arg))
        {
            _argumentType = ArgumentType.Path;
            return;
        }
        
        // Check if the argument is a file
        if (File.Exists(arg))
        {
            _argumentType = ArgumentType.File;
            return;
        }
        
        urlOrPath.ErrorMessage = $"Invalid argument: {arg}. See --help for more information.";
    }
}