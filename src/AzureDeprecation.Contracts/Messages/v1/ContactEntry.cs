using AzureDeprecation.Contracts.Enum;

namespace AzureDeprecation.Contracts.Messages.v1
{
    public class ContactEntry
    {
        public ContactType Type { get; set; }
        public string Data { get; set; }
    }
}