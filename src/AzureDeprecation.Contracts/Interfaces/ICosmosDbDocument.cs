namespace AzureDeprecation.Contracts.Interfaces
{
    public interface ICosmosDbDocument
    {
        public string Id { get; }
        public string SchemaVersion { get; }
    }
}
