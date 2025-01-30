namespace RepositoryLinter.Checks;

public abstract class Checker
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string TipToFix { get; set; }
    protected CheckStatus Status { get; set; }
    
    public abstract void Run();
    public abstract override string ToString();
}