using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureDeprecation.Contracts.Messages.v1
{
    public class NewAzureDeprecationV1Message
    {
        public string Title { get; set; }
        public List<InputTimeLineEntry> Timeline { get; set; } = new List<InputTimeLineEntry>();
        public Impact Impact { get; set; }
        public DraftNotice Notice { get; set; }
        public RequiredAction RequiredAction { get; set; }
        public Contact Contact { get; set; }
        public string AdditionalInformation { get; set; }

        public DateTimeOffset GetDueDate()
        {
            if (Timeline == null || Timeline.Any() == false)
            {
                throw new Exception("No timeline was provided");
            }

            if (Timeline.Count == 1)
            {
                return Timeline.First().Date;
            }

            return Timeline.Where(x => x.IsDueDate).Select(x=>x.Date).SingleOrDefault();
        }
    }
}
