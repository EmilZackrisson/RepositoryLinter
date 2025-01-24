using System.CommandLine;
using RepositoryLinter;


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
var pathToSaveArgument = new Argument<string?>("path-to-save-to", "Directory to save the repository to. If not provided, a temporary directory will be used.");
urlCommand.AddArgument(urlArgument);
urlCommand.AddArgument(pathToSaveArgument);
rootCommand.AddCommand(urlCommand);

urlArgument.AddValidator((url) =>
{
    if (!Uri.TryCreate(url.Tokens[0].Value, UriKind.Absolute, out _))
    {
        url.ErrorMessage = $"Invalid URL: {url.Tokens[0].Value}";
    }
});

urlCommand.SetHandler((url, disableCleanup) =>
{
    Console.WriteLine($"Linting URL: {url}");
    var git = new Git(new Uri(url))
    {
        SaveToDisk = !disableCleanup
    };
    git.Clone();
}, urlArgument, disableCleanupOption);




return await rootCommand.InvokeAsync(args);