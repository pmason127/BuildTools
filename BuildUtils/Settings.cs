namespace BuildUtils;

[Serializable]
public class Settings
{
    public Settings()
    {
        VersionMajor = 0;
        VersionMinor = 0;
        WorkingDirectory = String.Empty;
        SolutionDirectory = String.Empty;
        ExcludeFiles = new List<string>();
        ExcludeDirectories = new List<string>();

    }
    public string SolrVersion { get; set; }
    public int VersionMajor { get; set; }
    public int VersionMinor { get; set; }
    
    public string WorkingDirectory { get; set; }
    public string SolutionDirectory { get; set; }
    
    public IList<string> ExcludeFiles { get; set; }
    public  IList<string> ExcludeDirectories { get; set; }
}