namespace RepositoryLinter;

public class Logger
{
    public bool Verbose { get; set; } = false;
    
    public void Log(string message)
    {
        if (Verbose)
        {
            Console.WriteLine(message);
        }
    }
    
    public void Log(string message, params object[] args)
    {
        if (Verbose)
        {
            Console.WriteLine(message, args);
        }
    }
    
    public void LogError(string message)
    {
        Console.Error.WriteLine(message);
    }
}