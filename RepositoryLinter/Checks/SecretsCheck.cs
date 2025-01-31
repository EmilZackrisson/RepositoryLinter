using System.Diagnostics;
using Newtonsoft.Json;

namespace RepositoryLinter.Checks;

public class SecretsCheck(string pathToGitRepo) : Checker
{
    private readonly List<dynamic> _foundSecrets = [];
    public override void Run()
    {
        var directory = Path.GetFileName(pathToGitRepo);
        RunTrufflehogCommand($"filesystem {directory} --json --results=verified,unknown --fail");
    }
    
    public override string ToString()
    {
        var output = base.ToString();
        
        if (Status == CheckStatus.Green)
        {
            return output;
        }
        
        output += "\nSecrets found:\n";

        foreach (var secret in _foundSecrets)
        {
            var formatted = TrufflehogJsonToString(secret);
            if (formatted != null)
            {
                output += formatted + "\n";
            }
        }

        return output;
    }

    private string? TrufflehogJsonToString(dynamic json)
    {
        try
        {
            var output = $"{json.DetectorDescription} Found in {json.SourceMetadata.Data.Filesystem.file} at line {json.SourceMetadata.Data.Filesystem.line}";

            return output;
        }
        catch (Exception _)
        {
            return null;
        }
    }

    private void RunTrufflehogCommand(string command)
    {
        var parentDirectory = Path.GetDirectoryName(pathToGitRepo)!;
        
        Console.WriteLine($"Running trufflehog command: {command} in {parentDirectory}");
        
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
            _foundSecrets.Add(json);
        }
        
        p.WaitForExit();

        if (p.ExitCode != 183) return;
        
        Status = CheckStatus.Red;
    }
}