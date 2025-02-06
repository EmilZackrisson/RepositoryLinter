using System.Diagnostics;
using Newtonsoft.Json;

namespace RepositoryLinter.Checks;

public class SecretsCheck(string pathToGitRepo, GlobalConfiguration config) : Checker
{
    private List<dynamic> _foundSecretsJson = [];
    private readonly GitIgnore _gitIgnore = new(pathToGitRepo, config.GitIgnoreEnabled);
    private bool _fileHasBeenIgnored = false;
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
        
        var lines = output.Split("\n");

        foreach (var line in lines)
        {
            var json = JsonConvert.DeserializeObject<dynamic>(line);
            
            if (json == null) continue;
            
            _foundSecretsJson.Add(json);
        }
        
        p.WaitForExit();

        if (p.ExitCode != 183) return;
        
        Status = CheckStatus.Red;
    }
    
    private void RemoveIgnoredFiles()
    {
        var ignoredFiles = new List<dynamic>();
        foreach (var secret in _foundSecretsJson)
        {
            if (!_gitIgnore.IsIgnored(secret.SourceMetadata.Data.Filesystem.file.ToString())) continue;
            
            _fileHasBeenIgnored = true;
            ignoredFiles.Add(secret);
            _additionalInfo = "Secrets found in files that are ignored by .gitignore, and thus not being commited. Be cautious. If you want to search in these files, run the program with the --ignore-gitignore flag.";
        }
        
        // Remove ignored files from the list
        foreach (var ignoredFile in ignoredFiles)
        {
            _foundSecretsJson.Remove(ignoredFile);
        }

        if (_foundSecretsJson.Count == 0) return;
        
        Status = CheckStatus.Yellow;
    }
}