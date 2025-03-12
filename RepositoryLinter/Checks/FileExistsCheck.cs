namespace RepositoryLinter.Checks;

/// <summary>
/// Check if a file exists in the repository
/// </summary>
/// <param name="relativeFilePath">Relative file path from the Git repository. Supports wildcards. Not case-sensitive.</param>
/// <param name="pathToGitDirectory">Path to the local copy of the Git repository.</param>
public class FileExistsCheck(string relativeFilePath, string pathToGitDirectory) : Checker
{
    private bool _isEmpty;

    /// <summary>
    /// Status to return when the file is empty, aka 0 bytes. Default is Green.
    /// </summary>
    public CheckStatus StatusWhenEmpty { get; init; } = CheckStatus.Green;

    /// <summary>
    /// Recursively search for the file in the directory. Default is false.
    /// </summary>
    public bool RecursiveSearch { get; init; }

    public override void Run()
    {
        var fileName = Path.GetFileName(relativeFilePath);
        var directory = Path.GetDirectoryName(relativeFilePath);

        List<string> files;

        if (RecursiveSearch)
        {
            files = Directory.EnumerateFiles(Path.Join(pathToGitDirectory, directory), fileName,
                SearchOption.AllDirectories).ToList();
        }
        else
        {
            files = Directory.EnumerateFiles(Path.Join(pathToGitDirectory, directory), fileName,
                SearchOption.TopDirectoryOnly).ToList();
        }

        var exists = files.Count != 0;

        if (files.Count > 1)
        {
            Status = CheckStatus.Yellow;
            AdditionalInformation = $"\nMultiple files matching {fileName} found in the directory {directory}.";
            return;
        }

        if (exists)
        {
            _isEmpty = FilesAreEmpty(files);
        }

        if (exists && _isEmpty)
        {
            AdditionalInformation += $"File {fileName} is empty.";
            Status = StatusWhenEmpty;
            return;
        }

        Status = exists ? CheckStatus.Green : StatusWhenFailed;
    }

    /// <summary>
    /// Checks if a file is empty.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>True if the file is empty, otherwise false.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file is not found.</exception>
    private static bool FileIsEmpty(string path)
    {
        try
        {
            return new FileInfo(path).Length == 0;
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e);
            return true;
        }
    }

    /// <summary>
    /// Checks if all files in the list are empty.
    /// </summary>
    /// <param name="files">The list of file paths.</param>
    /// <returns>True if all files are empty, otherwise false.</returns>
    private static bool FilesAreEmpty(List<string> files)
    {
        return files.All(FileIsEmpty);
    }

    /// <summary>
    /// Returns a string representation of the check result.
    /// </summary>
    /// <returns>A string that represents the check result.</returns>
    public override string ToString()
    {
        var str = base.ToString();

        if (_isEmpty)
        {
            str += "\nFile is empty";
        }

        if (AdditionalInformation != "")
        {
            str += '\n' + AdditionalInformation;
        }

        return str;
    }
}