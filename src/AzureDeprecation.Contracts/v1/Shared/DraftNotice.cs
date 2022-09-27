namespace AzureDeprecation.Contracts.v1.Shared
{
    public class DraftNotice : ComposedInfo
    {
        public List<string> Links { get; set; } = new List<string>();
    }
}