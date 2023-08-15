using Microsoft.Extensions.Logging;

namespace BuildUtils;

public class App
{
    private Settings _settings = null;
    private ExecutionConfiguration _config = null;
    private ILogger _logger;

    public App(Settings settings, ExecutionConfiguration config, ILogger logger)
    {
        _config = config;
        _settings = settings;
        _logger = logger;
    }

    public Task<bool> Execute()
    {
        _logger.LogInformation("Build Utilities Running...");

        EnsurePackagingDirectoryExists();
        PublishWeb();

        return Task.FromResult<bool>(true);
    }

    private async Task LoadSettings(bool reload = false)
    {
        if (_settings != null && !reload)
            return;
    }

    //Previously read from a git commit date in versioninfo.json, however we never changed the commit so essentially the date was static
    //8/30/2017 is the date of that commit
    private readonly DateTime _revStartDate = new DateTime(2017, 08, 30, 0, 0, 0, DateTimeKind.Utc);

    public string GenerateVersion(DateTime buildDate, int? major = null, int? minor = null)
    {
        if (_revStartDate > buildDate)
            _logger.LogError("The date specified is invalid as it is earlier than base repo date.");

        var dateInt = (Int16)((buildDate - _revStartDate).TotalHours / 2);

        return $"{major ?? _settings.VersionMajor}.{minor ?? _settings.VersionMinor}.{dateInt}";
    }

    private void PublishWeb()
    {
        var webPath = SolutionPath("Telligent.Evolution.Web");
        var packagePath = PackagePath("Web");
        _logger.LogInformation("Verifying package WEB directory and creating...");
        EnsureDirectoryExists(packagePath,true);
        _logger.LogInformation("Publishing WEB files to package...");
        CopyDirectory(webPath,packagePath,true);
        
        _logger.LogInformation("Establishing empty filestorage...");
        EnsureDirectoryExists(Path.Combine(packagePath,"filestorage"),true);
    }
    private void EnsurePackagingDirectoryExists()
    {
        var path = ResolvePath("package");
        _logger.LogInformation("Checking packaging directory....");
       EnsureDirectoryExists(path,true);
    }

    private string PackagePath(string path)
    {
        return Path.Combine(ResolvePath("package"), path);
    }
    private string SolutionPath(string path)
    {
        return Path.Combine(_settings.SolutionDirectory, path);
    }
    private string ResolvePath(string path)
    {
        return Path.Combine(_settings.WorkingDirectory, path);
    }

    private void EnsureDirectoryExists(string path, bool recreate = false)
    {
        _logger.LogDebug($"----Directory Exists: {path}....");
        if (Directory.Exists(path))
        {
            _logger.LogDebug($"----Directory Exists: TRUE");
            if (recreate)
            {
                _logger.LogDebug($"----Recreation Requested: Deleting...");
                Directory.Delete(path,true);
                _logger.LogDebug($"----Recreation Requested: Creating...");
                Directory.CreateDirectory(path);
            }
            else
                _logger.LogDebug($"----Recreation Not Requested: Returning existing reference...");
                
        }
        else
        {
            Directory.CreateDirectory(path); 
        }
    }
    private void CopyDirectory(string sourceDir, string destinationDir, bool recursive,bool enforceCopyPolicy =true)
    {
        // Get information about the source directory
        _logger.LogDebug($"----DIRECTORY: COPY {sourceDir} to {destinationDir}, Recursive:{recursive}, Policy Enforced:{enforceCopyPolicy}...");
        var dir = new DirectoryInfo(sourceDir);

        if (IsDirectoryExcluded(dir))
        {
            _logger.LogDebug($"----DIRECTORY: Directory {dir.Name} is excluded, skipping...");
            return;
        }
            

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        _logger.LogDebug($"----DIRECTORY: Listing Sub-Directories...");
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        _logger.LogDebug($"----DIRECTORY: CREATING {destinationDir}...");
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            if (!IsFileExcluded(file))
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                _logger.LogDebug($"----FILE: COPY {file.FullName} to {targetFilePath}...");
                file.CopyTo(targetFilePath);
            }
          
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }

    private bool IsFileExcluded(FileInfo file)
    {
        var ext = file.Extension;
        var wildExt = "*" + ext;
        if (_settings.ExcludeFiles.Contains(wildExt))
        {
            _logger.LogDebug($"----FILE: File extension {file.Extension} is excluded, skipping...");
            return true;
        }


        if (_settings.ExcludeFiles.Contains(file.Name))
        {
            _logger.LogDebug($"----FILE: File {file.Name} is excluded, skipping...");
              return true;
        }
         
        return false;

    }
    private bool IsDirectoryExcluded(DirectoryInfo dir)
    {
        if (_settings.ExcludeDirectories.Contains(dir.Name))
            return true;

        return false;
    }
}