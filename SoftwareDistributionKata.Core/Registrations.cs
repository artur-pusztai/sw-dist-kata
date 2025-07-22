namespace SoftwareDistributionKata.Core;


public class Registration(string customer, string app, string country, string activationCode, string order)
{
    public string Customer { get; set; } = customer;
    public string App { get; set; } = app;
    public string Country { get; set; } = country;
    public string ActivationCode { get; set; } = activationCode;
    public string? InstalledVersion { get; set; }
    public string? HostGuid { get; set; }
    public DateTime? LastUpdate { get; set; } = DateTime.Now;
    public string Order { get; set; } = order;
}
