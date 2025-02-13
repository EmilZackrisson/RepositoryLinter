using System.Diagnostics;
using Newtonsoft.Json;

namespace RepositoryLinter.Checks;

public class SecretsCheck(string pathToGitRepo, GlobalConfiguration config) : Checker
{
    /// <summary>
    /// List of secrets found by trufflehog in JSON format.
    /// </summary>
    private readonly List<dynamic> _foundSecretsJson = [];
    
    /// <summary>
    /// GitIgnore object to check if files are ignored by .gitignore.
    /// </summary>
    private readonly GitIgnore _gitIgnore = new(pathToGitRepo, config.GitIgnoreEnabled);
    
    /// <summary>
    /// Additional information about the check. This is set if secrets are found in files that are ignored by .gitignore.
    /// </summary>
    private bool _fileHasBeenIgnored;
    
    /// <summary>
    /// Additional information about the check. This is set if secrets are found in files that are ignored by .gitignore.
    /// </summary>
    private string _additionalInfo = "";
    public override void Run()
    {
        var directory = Path.GetFileName(pathToGitRepo);
        RunTrufflehogCommand($"filesystem {directory} --json --results=verified,unknown --fail");
        RemoveIgnoredFiles();
    }
    
    public override string ToString()
    {
        var output = base.ToString();
        
        if (Status == CheckStatus.Green)
        {
            return output;
        }
        
        if (Status == CheckStatus.Yellow)
        {
            output += "\n" + _additionalInfo;
            return output;
        }
        
        output += "\nSecrets found:\n";

        foreach (var secret in _foundSecretsJson)
        {
            var formatted = TrufflehogJsonToString(secret);

            if (formatted == null)
            {
                continue;
            }
            
            output += formatted + "\n";
        }
        
        if (_fileHasBeenIgnored)
        {
            output += _additionalInfo;
        }

        return output;
    }

    /// <summary>
    /// Converts a trufflehog JSON object to a string. Returns null if the JSON object is invalid.
    /// </summary>
    /// <param name="json">JSON secrets found line from Trufflehog</param>
    /// <returns>A string in the format {filename} line {line_nr} - {description_of_secret}</returns>
    private string? TrufflehogJsonToString(dynamic json)
    {
        try
        {
            var output = $"{json.SourceMetadata.Data.Filesystem.file} line {json.SourceMetadata.Data.Filesystem.line} - {json.DetectorDescription}";

            return output;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Run the trufflehog command with the given command. The output is stored in the _foundSecretsJson list. Status is set to Red if the command returns 183, which means secrets were found.
    /// </summary>
    /// <param name="command">Command</param>
    /// <exception cref="Exception">Throws if trufflehog command couldn't start</exception>
    private void RunTrufflehogCommand(string command)
    {
        var parentDirectory = Path.GetDirectoryName(pathToGitRepo)!;
        
        var p = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "trufflehog",
                Arguments = command,
                WorkingDirectory = parentDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        var started = p.Start();
        if (!started)
        {
            throw new Exception("Failed to start trufflehog command");
        }
        
        // Read the output from the command
        var output = p.StandardOutput.ReadToEnd();
        
        // Split the output into lines and parse the JSON
        var lines = output.Split("\n");
        foreach (var line in lines)
        {
            var json = JsonConvert.DeserializeObject<dynamic>(line);
            
            if (json == null) continue;
            
            _foundSecretsJson.Add(json);
        }
        
        p.WaitForExit();

        // If the exit code is 183, secrets were found
        if (p.ExitCode != 183) return;
        
        Status = CheckStatus.Red;
    }
    
    /// <summary>
    /// Removes secrets found from _foundSecretsJson if the file is ignored by .gitignore. Status is set to Yellow if all secrets are found in ignored files.
    /// </summary>
    private void RemoveIgnoredFiles()
    {
        var ignoredFiles = new List<dynamic>();
        foreach (var secret in _foundSecretsJson.Where(secret => _gitIgnore.IsIgnored(secret.SourceMetadata.Data.Filesystem.file.ToString())))
        {
            _fileHasBeenIgnored = true;
            ignoredFiles.Add(secret);
            _additionalInfo = "Secrets found in files that are ignored by .gitignore, and thus not being commited. Be cautious. If you want to search in these files, run the program with the --ignore-gitignore flag.";
        }
        
        // Remove ignored files from the list
        foreach (var ignoredFile in ignoredFiles)
        {
            _foundSecretsJson.Remove(ignoredFile);
        }

        if (_foundSecretsJson.Count != 0) return;
        Status = CheckStatus.Yellow;
    }
}