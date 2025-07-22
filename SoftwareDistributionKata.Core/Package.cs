namespace SoftwareDistributionKata.Core;

public class Package(string app, string version, bool rollout, List<string> clearedCountries)
{
    public string App { get; set; } = app;
    public string Version { get; set; } = version;
    public bool Rollout { get; set; } = rollout;
    public List<string> ClearedCountries { get; set; } = clearedCountries ?? [];
}

