using System.ComponentModel.DataAnnotations;

namespace RepositoryLinter.Checks;

public class CheckerConfiguration
{
    [Required] public string Name { get; set; } = string.Empty;

    [Required] public bool AllowedToFail { get; set; } = false;
}