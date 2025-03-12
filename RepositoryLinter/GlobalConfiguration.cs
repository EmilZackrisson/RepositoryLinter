using RepositoryLinter.Checks;
using YamlDotNet.Serialization;

namespace RepositoryLinter;

public class GlobalConfiguration
{
    /// <summary>
    /// Truncate the output of the checks to 10 lines. Default is true.
    /// </summary>
    public bool TruncateOutput { get; set; } = true;

    /// <summary>
    /// Enable or disable the .gitignore file. Default is true.
    /// </summary>
    public bool GitIgnoreEnabled { get; set; } = true;

    /// <summary>
    /// Clean up cloned repositories after the program has finished. Local repositories are never deleted. Default is true.
    /// </summary>
    public bool CleanUp { get; set; } = true;

    /// <summary>
    /// Path to save cloned Git repositories. Default is random temporary path.
    /// </summary>
    public string PathToSaveGitRepos { get; set; } = Path.Join(Path.GetTempPath(), "RepositoryLinter");

    [YamlIgnore] public dynamic? DynamicConfiguration { get; set; }

    public List<CheckerConfiguration> Checks { get; set; } = new();

    public bool IsFromConfigFile { get; set; }

    public GlobalConfiguration(string[] programArguments)
    {
        // Get index of "--config" argument
        var configIndex = Array.IndexOf(programArguments, "--config");

        // If the argument is not found, return
        if (configIndex == -1)
        {
            return;
        }

        // If the argument is found, get the path to the configuration file
        var configPath = programArguments[configIndex + 1];

        // Read the configuration file and set the properties
        ReadAndSetConfiguration(configPath);
    }

    public GlobalConfiguration()
    {
    }

    private void ReadAndSetConfiguration(string path)
    {
        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();
        var yamlString = File.ReadAllText(path);
        var config = deserializer.Deserialize<GlobalConfiguration>(yamlString);

        TruncateOutput = config.TruncateOutput;
        GitIgnoreEnabled = config.GitIgnoreEnabled;
        CleanUp = config.CleanUp;
        PathToSaveGitRepos = config.PathToSaveGitRepos;
        Checks = config.Checks;
        DynamicConfiguration = config.DynamicConfiguration;
        IsFromConfigFile = true;
    }
}