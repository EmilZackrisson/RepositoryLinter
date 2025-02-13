using System.CommandLine.Invocation;

namespace RepositoryLinter.Middlewares;

public class CommandLineBuilderMiddleware(GlobalConfiguration config)
{
    public async Task GetOptionsIntoGlobalConfiguration(InvocationContext context, Func<InvocationContext, Task> next)
    {
        var tokens = context.ParseResult.Tokens;
        var args = tokens.Select(t => t.Value).ToArray();
    
        // Set global options
        config.GitIgnoreEnabled = !args.Contains("--ignore-gitignore");
        config.TruncateOutput = !args.Contains("--disable-truncate");
        config.CleanUp = !args.Contains("--disable-cleanup");
        
        await next(context);
    }
}