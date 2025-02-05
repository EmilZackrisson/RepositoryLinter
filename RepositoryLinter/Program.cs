using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using RepositoryLinter;
using RepositoryLinter.Checks;


// Root command
var rootCommand = new RootCommand("Simple CLI tool to lint git repositories");

// Global options
var verboseOption = new Option<bool>("--verbose", "Show verbose output");
rootCommand.AddGlobalOption(verboseOption);
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
    config.Verbose = args.Contains("--verbose");
    config.GitIgnoreEnabled = !args.Contains("--ignore-gitignore");
    config.TruncateOutput = !args.Contains("--disable-truncate");
    config.CleanUp = !args.Contains("--disable-cleanup");

    await next(context);
});

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

    var git = new Git(new Uri(url), config);
    git.Clone();
    
    // Run linting pipeline
    Run(git);
    
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
    var git = new Git(path);

    Run(git);

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
            var git = new Git(new Uri(line), config);
            git.Clone();
            Run(git);
        }
        else
        {
            var git = new Git(line)
            {
                SaveToDisk = true
            };
            Run(git);
        }
    }
}, batchFileArgument);

//return await rootCommand.InvokeAsync(args);

commandLineBuilder.UseDefaults();
var parser = commandLineBuilder.Build();
await parser.InvokeAsync(args);

void Run(Git git)
{
    // Run linting pipeline
    var linter = new Linter(git, config);
    
    linter.AddCheck(new FileExistsCheck("README.md", git.PathToGitDirectory)
    {
        Name = "README.md exists",
        Description = "Check if README.md exists",
        TipToFix = "Create a README.md file.",
    });
    
    linter.AddCheck(new FileExistsCheck("LICENCE.*", git.PathToGitDirectory)
    {
        Name = "LICENCE file does not exist",
        Description = "Check if LICENCE exists",
        TipToFix = "Create a LICENCE file. Read more about licenses at https://choosealicense.com/ and https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository",
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
        Name = "Test string exists",
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