using RepositoryLinter.Checks;

namespace RepositoryLinter;

public class Linter(Git git, GlobalConfiguration config)
{
    private readonly List<Checker> _checks = [];

    /// <summary>
    /// Add a check to the linter.
    /// </summary>
    /// <param name="check">A checker</param>
    public void AddCheck(Checker check)
    {
        _checks.Add(check);
    }
    
    /// <summary>
    /// Run all checks added to the linter.
    /// </summary>
    public void Run()
    {
        foreach (var check in _checks)
        {
            check.Run();
        }
    }
    
    /// <summary>
    /// Get the status code of the linter. Returns 1 if any check has a red status, otherwise 0.
    /// </summary>
    /// <returns></returns>
    public int StatusCode()
    {
        return _checks.Any(check => check.Status == CheckStatus.Red) ? 1 : 0;
    }
    
    /// <summary>
    /// Prints a report of the results of the checks.
    /// </summary>
    public void PrintResults()
    {
        // Print "-" for the entire width of the console
        var dashes = new string('-', Console.WindowWidth);
        Console.WriteLine(dashes);
        
        Console.WriteLine($"Report for {git.RepositoryName}\n");
        Console.WriteLine($"Number of commits: {git.GetCommitCount()}\n");
        
        var contributors = git.GetContributors();
        Console.WriteLine("Contributors:");
        foreach (var contributor in contributors)
        {
            Console.WriteLine(contributor);
        }
        Console.WriteLine();
        
        foreach (var check in _checks)
        {
            var str = check.ToString();
            var list = str.Split("\n");

            if (list.Length > 13 && config.TruncateOutput)
            {
                Console.WriteLine(string.Join("\n", list.Take(10)));
                Console.WriteLine("...");
                Console.WriteLine(string.Join("\n", list.TakeLast(3)));
            }
            else
            {
                Console.WriteLine(str);
            }
            
            // Print a newline between checks, unless it's the last check
            if (check != _checks.Last())
            {
                Console.WriteLine();
            }
        }
        Console.WriteLine(dashes);
    }
}