namespace RepositoryLinter.Checks;

public abstract class Checker
{
    /// <summary>
    /// Name of the check. This should be a short and concise name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Short description of the check. This should be a short and concise message.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gives a tip to fix the issue if the check fails. This should be a short and concise message.
    /// </summary>
    public required string TipToFix { get; init; }

    /// <summary>
    /// Current status of the check.
    /// </summary>
    public CheckStatus Status { get; set; }

    /// <summary>
    /// Status to set when the check fails. Default is CheckStatus.Red.
    /// </summary>
    public CheckStatus StatusWhenFailed { get; set; } = CheckStatus.Red;

    /// <summary>
    /// Run the check and set the status based on the result. The status should be set to CheckStatus.Green if the check passes, and CheckStatus.Red if the check fails. You can also set the status to CheckStatus.Yellow if the check is inconclusive.
    /// </summary>
    public abstract void Run();

    /// <summary>
    /// Returns a nicely formatted string representation of the Checker with the status icon, name, description, and tip to fix.
    /// </summary>
    /// <returns>A nicely formatted string representation of the Checker</returns>
    public override string ToString()
    {
        return Status == CheckStatus.Green
            ? $"{CheckStatusExtensions.ToIcon(Status)}   {Name}"
            : $"{CheckStatusExtensions.ToIcon(Status)}   {Name}\nDescription: {Description}\nTip to fix: {TipToFix}";
    }
}