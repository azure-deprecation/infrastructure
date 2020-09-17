using System.Collections.Generic;

namespace AzureDeprecation.Contracts.Messages.v1
{
    public class Notice
    {
        public string OfficialReport { get; set; }
        public string AdditionalInfo { get; set; }
        public List<string> Links { get; set; } = new List<string>();
    }
}