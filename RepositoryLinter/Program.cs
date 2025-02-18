using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using RepositoryLinter;
using RepositoryLinter.Handlers;

// Root command
var rootCommand = new RootCommand("Simple CLI tool to lint git repositories");

// Add trailing slash to argument if its a http(s) URL. This is needed because C# args does some weird stuff.
if (args[0].StartsWith("http://") || args[0].StartsWith("https://"))
{
    args[0] = args[0].TrimEnd('/') + "/";
}

var rootArgument = new Argument<string>("url-or-path", "URL or path to lint. Path can be a directory or a batch file containing URLs or paths, one per line.");
rootCommand.AddArgument(rootArgument);

// Global options
var disableCleanupOption = new Option<bool>("--disable-cleanup", "Do not delete the cloned git repository");
rootCommand.AddGlobalOption(disableCleanupOption);
var disableTruncateOption = new Option<bool>("--disable-truncate", "Do not truncate the output. By default, the output is truncated to 10 lines per check.");
rootCommand.AddGlobalOption(disableTruncateOption);
var ignoreGitIgnoreOption = new Option<bool>("--ignore-gitignore", "Ignore .gitignore. Includes all files in the search.");
rootCommand.AddGlobalOption(ignoreGitIgnoreOption);

var pathToSaveOption = new Option<string?>("--path-to-save-to", "Directory to save the repository to. If not provided, /tmp/repolinter/git will be used.");
rootCommand.AddOption(pathToSaveOption);

// Create global config
var config = new GlobalConfiguration();

var commandLineBuilder = new CommandLineBuilder(rootCommand);

// Middleware to set global options
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

// Create Lint Pipeline Runner
var runner = new LintRunner(config);

// Create Root Command Handler
var rootCommandHandler = new RootCommandHandler(runner, config);
rootArgument.AddValidator(rootCommandHandler.Validate);
rootCommand.SetHandler((urlOrPath) => rootCommandHandler.Handle(urlOrPath), rootArgument);


commandLineBuilder.UseDefaults();
var parser = commandLineBuilder.Build();
await parser.InvokeAsync(args);