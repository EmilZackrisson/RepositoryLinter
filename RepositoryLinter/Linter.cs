using RepositoryLinter.Checks;

namespace RepositoryLinter;

public class Linter(Git git)
{
    private readonly Git _git = git;
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
        Console.WriteLine($"Report for {_git.RepositoryName}\n");
        
        foreach (var check in _checks)
        {
            Console.WriteLine(check);
        }
    }
}