using System.Diagnostics;
using RepositoryLinter.Exceptions;

namespace RepositoryLinter;

public class GitCheckIgnore(string pathToGitDirectory)
{
    public bool IsIgnored(string path)
    {
        var p = new Process
        {
            StartInfo =
            {
                FileName = "git",
                Arguments = $"check-ignore {path}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = pathToGitDirectory
            }
        };

        var started = p.Start();
        if (!started)
        {
            throw new GitException("Failed to run git check-ignore");
        }

        var output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        return output.Contains(path);
    }
}