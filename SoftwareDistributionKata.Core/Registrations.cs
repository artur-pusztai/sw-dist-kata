namespace SoftwareDistributionKata.Core;


public class Registration
{
    public string Customer { get; set; }
    public string App { get; set; }
    public string Country { get; set; }
    public string ActivationCode { get; set; }
    public string? InstalledVersion { get; set; }
    public string? HostGuid { get; set; }
    public DateTime? LastUpdate { get; set; }
    public string Order { get; set; } // New field for sales order

    public Registration(string customer, string app, string country, string activationCode, string order)
    {
        Customer = customer;
        App = app;
        Country = country;
        ActivationCode = activationCode;
        Order = order;
        LastUpdate = DateTime.Now;
    }
}
