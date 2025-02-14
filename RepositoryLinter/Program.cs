using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using RepositoryLinter;
using RepositoryLinter.Handlers;

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
var batchCommand = new Command("batch", "Run lint on multiple repositories");
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

/* // TODO: Implement middleware in own class
var commandLineBuilderMiddleware = new CommandLineBuilderMiddleware(config);
commandLineBuilder.AddMiddleware(async (context, next) =>
    await commandLineBuilderMiddleware.GetOptionsIntoGlobalConfiguration(context, next));
*/

// Create Lint Pipeline Runner
var runner = new LintRunner(config);

// Create URL command handler
var urlCommandHandler = new UrlCommandHandler(runner, config);
urlArgument.AddValidator(UrlCommandHandler.Validate);
urlCommand.SetHandler((url, pathToSave) => urlCommandHandler.Handle(url, pathToSave), urlArgument, pathToSaveOption);

// Create path command handler
var pathCommandHandler = new PathCommandHandler(runner);
pathArgument.AddValidator(PathCommandHandler.Validate);
pathCommand.SetHandler((path) => pathCommandHandler.Handle(path), pathArgument);

// Create batch file command handler
var batchCommandHandler = new BatchCommandHandler(runner, config);
batchFileArgument.AddValidator(BatchCommandHandler.Validate);
batchCommand.SetHandler((repos) => batchCommandHandler.Handle(repos), batchFileArgument);

commandLineBuilder.UseDefaults();
var parser = commandLineBuilder.Build();
await parser.InvokeAsync(args);