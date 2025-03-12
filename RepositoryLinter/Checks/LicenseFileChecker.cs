using System.Text;

namespace RepositoryLinter.Checks;

public class LicenseFileChecker : Checker
{
    private readonly string _directory;
    public CheckStatus StatusWhenEmpty { get; init; } = CheckStatus.Red;

    public LicenseFileChecker(string directory)
    {
        _directory = directory;
        Status = CheckStatus.Red;
    }

    public override void Run()
    {
        var licenseFiles = Directory.EnumerateFiles(_directory, "LICENSE*", SearchOption.TopDirectoryOnly).ToList();

        if (licenseFiles.Count == 1 && !FilesAreEmpty(licenseFiles))
        {
            // License file found and not empty
            Status = CheckStatus.Green;
            return;
        }

        if (licenseFiles.Count == 1 && FilesAreEmpty(licenseFiles))
        {
            // License file found but is empty
            Status = StatusWhenEmpty;
            AdditionalInformation = "License file is empty.";
            return;
        }

        if (licenseFiles.Count > 1)
        {
            // Multiple license files found
            Status = CheckStatus.Yellow;
            AdditionalInformation = "Multiple license files found.";
            return;
        }

        // Check if there is a LICENSE directory that contains the license file
        var licenseDirectories = Directory.EnumerateDirectories(_directory, "LICENSE*", SearchOption.TopDirectoryOnly)
            .ToList();

        if (licenseDirectories.Count == 0) return; // No LICENSE directory found

        // Check if the license file is in the LICENSE directory
        var licenseDirectory = licenseDirectories[0];
        var licenseFilesInDirectory =
            Directory.EnumerateFiles(licenseDirectory, "LICENSE*", SearchOption.TopDirectoryOnly).ToList();

        var filesAreEmpty = FilesAreEmpty(licenseFilesInDirectory);

        if (licenseFilesInDirectory.Count != 0 && !filesAreEmpty)
        {
            // License file found in LICENSE directory and is not empty
            Status = CheckStatus.Green;
        }

        if (licenseFilesInDirectory.Count != 0 && filesAreEmpty)
        {
            // License file found in LICENSE directory but is empty
            Status = StatusWhenEmpty;
            AdditionalInformation = $"License file in {licenseDirectory} is empty.";
        }
    }

    private static bool FilesAreEmpty(List<string> files)
    {
        return files.Any(file => new FileInfo(file).Length == 0);
    }

    public override string ToString()
    {
        var builder = new StringBuilder(base.ToString());

        switch (Status)
        {
            case CheckStatus.Green:
                return builder.ToString();
            case CheckStatus.Yellow:
                builder.Append(Environment.NewLine);
                builder.Append(AdditionalInformation);
                return builder.ToString();
            default:
                builder.Append(Environment.NewLine);
                builder.Append("License file not found.");
                return builder.ToString();
        }
    }
}