namespace RepositoryLinter;

public enum CheckStatus
{
    Green,
    Yellow,
    Red,
    Gray
}

public static class CheckStatusExtensions
{
    public static string ToIcon(CheckStatus status)
    {
        return status switch
        {
            CheckStatus.Green => "✅",
            CheckStatus.Yellow => "⚠️",
            CheckStatus.Red => "❌",
            CheckStatus.Gray => "🔘",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}