namespace AzureDeprecation.APIs.REST.Contracts;

public enum ImpactType
{
    Unknown,
    None,
    Limited,
    UpgradeRequired,
    MigrationRequired,
    ShutdownWithoutAlternative
}