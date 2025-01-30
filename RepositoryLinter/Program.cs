using System.CommandLine;
using RepositoryLinter;
using RepositoryLinter.Checks;


// Root command
var rootCommand = new RootCommand("Simple CLI tool to lint git repositories");

// Global options
var verboseOption = new Option<bool>("--verbose", "Show verbose output");
rootCommand.AddGlobalOption(verboseOption);
var disableCleanupOption = new Option<bool>("--disable-cleanup", "Do not delete the cloned git repository");
rootCommand.AddGlobalOption(disableCleanupOption);

// Url command
var urlCommand = new Command("url", "Run lint on a URL");
var urlArgument = new Argument<string>("url", "URL to lint");
var pathToSaveOption = new Option<string?>("--path-to-save-to", "Directory to save the repository to. If not provided, a temporary directory will be used.");
urlCommand.AddArgument(urlArgument);
urlCommand.AddOption(pathToSaveOption);
rootCommand.AddCommand(urlCommand);

// Validate URL argument
urlArgument.AddValidator((url) =>
{
    if (!Uri.TryCreate(url.Tokens[0].Value, UriKind.Absolute, out _))
    {
        url.ErrorMessage = $"Invalid URL: {url.Tokens[0].Value}";
    }
});

// Set handler for URL command
urlCommand.SetHandler((url, disableCleanup, pathToSave) =>
{
    Console.WriteLine($"Linting URL: {url}");
    
    var pathToSaveTo = pathToSave ?? "/tmp/repolinter/git";
    
    var git = new Git(new Uri(url), pathToSaveTo)
    {
        SaveToDisk = !disableCleanup
    };
    git.Clone();
    
    // Run linting pipeline
    
}, urlArgument, disableCleanupOption, pathToSaveOption);

// Path command
var pathCommand = new Command("path", "Run lint on a local path");
var pathArgument = new Argument<string>("path", "Path to lint");
pathCommand.AddArgument(pathArgument);
rootCommand.AddCommand(pathCommand);

// Validate path argument
pathArgument.AddValidator((path) =>
{
    if (!Directory.Exists(path.Tokens[0].Value))
    {
        path.ErrorMessage = $"Invalid path: {path.Tokens[0].Value}";
    }
});

// Set handler for path command
pathCommand.SetHandler((path) =>
{
    Console.WriteLine($"Linting path: {path}");
    var git = new Git(path)
    {
        SaveToDisk = true
    };

    // Run linting pipeline
    var linter = new Linter(git);
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
    
    linter.AddCheck(new SearchForStringCheck("test", git.PathToGitDirectory)
    {
        Name = "Test string exists",
        Description = "Check if the string 'test' exists in the repository",
        TipToFix = "Remove the string 'test' from the repository",
        StatusWhenFailed = CheckStatus.Red,
        InvertResult = true
    });
    
    linter.Run();
    linter.PrintResults();

}, pathArgument);

return await rootCommand.InvokeAsync(args);