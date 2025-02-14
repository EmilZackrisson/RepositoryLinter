namespace RepositoryLinter.Checks;

public class LicenseFileChecker(string directory) : Checker
{
    public override void Run()
    {
        var licenseFiles = Directory.EnumerateFiles(directory, "LICENSE*", SearchOption.TopDirectoryOnly).ToList();

        if (licenseFiles.Count != 0 && !FilesAreEmpty(licenseFiles))
        {
            // License file found and not empty
            Status = CheckStatus.Green;
        }
        
        // Check if there is a LICENSE directory that contains the license file
        var licenseDirectories = Directory.EnumerateDirectories(directory, "LICENSE*", SearchOption.TopDirectoryOnly).ToList();
        
        if (licenseDirectories.Count == 0) return; // No LICENSE directory found
        
        // Check if the license file is in the LICENSE directory
        var licenseDirectory = licenseDirectories[0];
        var licenseFilesInDirectory = Directory.EnumerateFiles(licenseDirectory, "LICENSE*", SearchOption.TopDirectoryOnly).ToList();
            
        if (licenseFilesInDirectory.Count != 0 && !FilesAreEmpty(licenseFilesInDirectory))
        {
            // License file found in LICENSE directory and is not empty
            Status = CheckStatus.Green;
        }
    }
    
    private bool FilesAreEmpty(List<string> files)
    {
        return files.Any(file => new FileInfo(file).Length == 0);
    }
}