using System.Diagnostics;
using RepositoryLinter.Exceptions;

namespace RepositoryLinter;

public class GitCheckIgnore(string pathToGitDirectory, bool enabled = true)
{
    public bool IsIgnored(string path)
    {
        if (!enabled)
        {
            return false;
        }

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