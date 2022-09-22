using AzureDeprecation.Contracts.Enum;

namespace AzureDeprecation.APIs.REST.Contracts;

public class ContactEntry
{
    public ContactType Type { get; set; }
    public string? Data { get; set; }
}