using System;

namespace AzureDeprecation.Contracts.Messages.v1
{
    public class NewAzureDeprecationV1Message
    {
        public string Title { get; set; }
        public DateTimeOffset DueOn { get; set; }
        public Impact Impact { get; set; }
        public Notice Notice { get; set; }
        public RequiredAction RequiredAction { get; set; }
        public Contact Contact { get; set; }
        public string AdditionalInformation { get; set; }
    }
}
