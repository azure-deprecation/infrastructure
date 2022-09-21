using AzureDeprecation.Contracts.Enum;

namespace AzureDeprecation.Contracts.v1.Shared
{
    public class ContactEntry
    {
        public ContactType Type { get; set; }
        public string? Data { get; set; }
    }
}