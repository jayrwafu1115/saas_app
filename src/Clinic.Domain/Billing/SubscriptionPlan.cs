using Clinic.Domain.Common;

namespace Clinic.Domain.Billing;

public sealed class SubscriptionPlan : BaseEntity
{
    private SubscriptionPlan()
    {
        Name = string.Empty;
        Code = string.Empty;
        FeaturesJson = "{}";
    }

    public SubscriptionPlan(string name, string code, decimal monthlyPricePhp, int maxUsers, int maxDoctors, int maxLocations, int maxPatients, int trialDays, string featuresJson)
    {
        Name = name.Trim();
        Code = code.Trim().ToLowerInvariant();
        MonthlyPricePhp = monthlyPricePhp;
        MaxUsers = maxUsers;
        MaxDoctors = maxDoctors;
        MaxLocations = maxLocations;
        MaxPatients = maxPatients;
        TrialDays = trialDays;
        FeaturesJson = featuresJson;
        IsActive = true;
    }

    public string Name { get; private set; }
    public string Code { get; private set; }
    public decimal MonthlyPricePhp { get; private set; }
    public int MaxUsers { get; private set; }
    public int MaxDoctors { get; private set; }
    public int MaxLocations { get; private set; }
    public int MaxPatients { get; private set; }
    public int TrialDays { get; private set; }
    public string FeaturesJson { get; private set; }
    public bool IsActive { get; private set; }
}
