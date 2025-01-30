using RepositoryLinter.Checks;

namespace RepositoryLinter;

public class Linter(Git git)
{
    private readonly List<Checker> _checks = [];

    public void AddCheck(Checker check)
    {
        _checks.Add(check);
    }
    
    public void Run()
    {
        foreach (var check in _checks)
        {
            check.Run();
        }
    }
    
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