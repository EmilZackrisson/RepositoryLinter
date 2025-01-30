using RepositoryLinter.Checks;

namespace RepositoryLinter;

public class Linter(Git git)
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
    /// Prints a report of the results of the checks.
    /// </summary>
    public void PrintResults()
    {
        Console.WriteLine($"Report for {git.RepositoryName}\n");
        Console.WriteLine($"Number of commits: {git.GetCommitCount()}");
        
        var contributors = git.GetContributors();
        Console.WriteLine("Contributors:");
        foreach (var contributor in contributors)
        {
            Console.WriteLine(contributor);
        }
        
        foreach (var check in _checks)
        {
            Console.WriteLine(check);
        }
    }
}