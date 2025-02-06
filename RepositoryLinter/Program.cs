using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using RepositoryLinter;
using RepositoryLinter.Checks;


// Root command
var rootCommand = new RootCommand("Simple CLI tool to lint git repositories");

// Global options
var disableCleanupOption = new Option<bool>("--disable-cleanup", "Do not delete the cloned git repository");
rootCommand.AddGlobalOption(disableCleanupOption);
var disableTruncateOption = new Option<bool>("--disable-truncate", "Do not truncate the output. By default, the output is truncated to 10 lines per check.");
rootCommand.AddGlobalOption(disableTruncateOption);
var ignoreGitIgnoreOption = new Option<bool>("--ignore-gitignore", "Ignore .gitignore. Includes all files in the search.");
rootCommand.AddGlobalOption(ignoreGitIgnoreOption);

// Url command
var urlCommand = new Command("url", "Run lint on a URL");
var urlArgument = new Argument<string>("url", "URL to lint");
var pathToSaveOption = new Option<string?>("--path-to-save-to", "Directory to save the repository to. If not provided, /tmp/repolinter/git will be used.");
urlCommand.AddArgument(urlArgument);
urlCommand.AddOption(pathToSaveOption);
rootCommand.AddCommand(urlCommand);

// Path command
var pathCommand = new Command("path", "Run lint on a local path");
var pathArgument = new Argument<string>("path", "Path to lint");
pathCommand.AddArgument(pathArgument);
rootCommand.AddCommand(pathCommand);

// Batch file command
var batchCommand = new Command("file", "Run lint on multiple repositories");
var batchFileArgument = new Argument<FileInfo>("file", "File with a list of repositories to lint. Each line should be a URL or a path to a local repository.");
batchCommand.AddArgument(batchFileArgument);
rootCommand.AddCommand(batchCommand);

// Create global config
var config = new GlobalConfiguration();

var commandLineBuilder = new CommandLineBuilder(rootCommand);
commandLineBuilder.AddMiddleware(async (context, next) =>
{
    var tokens = context.ParseResult.Tokens;
    var args = tokens.Select(t => t.Value).ToArray();
    
    // Set global options
    config.GitIgnoreEnabled = !args.Contains("--ignore-gitignore");
    config.TruncateOutput = !args.Contains("--disable-truncate");
    config.CleanUp = !args.Contains("--disable-cleanup");

    await next(context);
});

// Create Lint Runner
var runner = new LintRunner(config);

// Validate URL argument
urlArgument.AddValidator((url) =>
{
    if (!Uri.TryCreate(url.Tokens[0].Value, UriKind.Absolute, out _))
    {
        url.ErrorMessage = $"Invalid URL: {url.Tokens[0].Value}";
    }
});

// Set handler for URL command
urlCommand.SetHandler((url, pathToSave) =>
{
    Console.WriteLine($"Linting URL: {url}");

    config.PathToSaveGitRepos = pathToSave ?? config.PathToSaveGitRepos;

    try
    {
        var git = new Git(new Uri(url), config);
        git.Clone();
        
        // Run linting pipeline
        runner.Run(git);
    }
    catch (Exception e)
    {
        Console.Error.WriteLine($"Failed to lint: {url}");
        Console.Error.WriteLine(e.Message);
        Environment.Exit(1);
    }
}, urlArgument, pathToSaveOption);


// Validate path argument
pathArgument.AddValidator((path) =>
{
    if (!Directory.Exists(path.Tokens[0].Value))
    {
        path.ErrorMessage = $"Invalid path: {path.Tokens[0].Value}";
    }
});

// Validate batch argument
batchFileArgument.AddValidator((repo) =>
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
});

// Set handler for path command
pathCommand.SetHandler((path) =>
{
    Console.WriteLine($"Linting path: {path}");

    try
    {
        var git = new Git(path);
        runner.Run(git);
    }
    catch (Exception e)
    {
        Console.Error.WriteLine($"Failed to lint: {path}", e);
        Environment.Exit(1);
    }
}, pathArgument);

// Set handler for batch command
batchCommand.SetHandler((repos) =>
{
    Console.WriteLine($"Linting batch file: {repos}");
    var lines = File.ReadAllLines(repos.FullName);
    foreach (var line in lines)
    {
        var isUrl = Uri.TryCreate(line, UriKind.Absolute, out _);
        if (isUrl)
        {
            // Run linter on URL
            try
            {
                var git = new Git(new Uri(line), config);
                git.Clone();
                runner.Run(git);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Failed to lint: {line}", e);
            }
        }
        else
        {
            try
            {
                // Run linter on local path
                var git = new Git(line)
                {
                    SaveToDisk = true
                };
                runner.Run(git);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Failed to lint: {line}", e);
            }
        }
    }
}, batchFileArgument);


commandLineBuilder.UseDefaults();
var parser = commandLineBuilder.Build();
await parser.InvokeAsync(args);