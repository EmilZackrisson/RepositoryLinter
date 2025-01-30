namespace RepositoryLinter.Checks;

public abstract class Checker
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string TipToFix { get; set; }
    protected CheckStatus Status { get; set; }
    public CheckStatus StatusWhenFailed { get; set; } = CheckStatus.Red;
    
    public abstract void Run();
    
    public override string ToString()
    {
        return Status == CheckStatus.Green ? $"{CheckStatusExtensions.ToIcon(Status)} {Name}" : $"{CheckStatusExtensions.ToIcon(Status)} {Name}\nDescription: {Description}\nTip to fix: {TipToFix}";
    }
}