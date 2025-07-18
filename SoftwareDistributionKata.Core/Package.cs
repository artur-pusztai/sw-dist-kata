namespace SoftwareDistributionKata.Core;

public class Package
{
    public string App { get; set; }
    public string Version { get; set; }
    public bool Rollout { get; set; }
    public List<string> ClearedCountries { get; set; }

    public Package(string app, string version, bool rollout, List<string> clearedCountries)
    {
        App = app;
        Version = version;
        Rollout = rollout;
        ClearedCountries = clearedCountries ?? new List<string>();
    }
}

