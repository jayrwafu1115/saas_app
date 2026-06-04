namespace Clinic.Infrastructure.Billing;

public sealed class PhilippinesBillingOptions
{
    public const string SectionName = "PhilippinesBilling";

    public string CheckoutBaseUrl { get; set; } = "https://checkout.clinic.local/ph";
}
