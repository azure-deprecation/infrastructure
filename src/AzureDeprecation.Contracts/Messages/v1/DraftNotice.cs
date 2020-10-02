using System.Collections.Generic;

namespace AzureDeprecation.Contracts.Messages.v1
{
    public class DraftNotice : ComposedInfo
    {
        public List<string> Links { get; set; } = new List<string>();
    }
}