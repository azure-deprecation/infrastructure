using System.Collections.Generic;

namespace AzureDeprecation.Contracts.Messages.v1
{
    public class DeprecationInfo
    {
        public string Title { get; set; }
        public List<TimeLineEntry> Timeline { get; set; } = new List<TimeLineEntry>();
        public Impact Impact { get; set; }
        public Notice Notice { get; set; }
        public string RequiredAction { get; set; }
        public Contact Contact { get; set; }
        public string AdditionalInformation { get; set; }
    }
}