namespace IoTHubManagement.Settings;

public class CosmosDbSettings
{
    public const string SectionName = "CosmosDb";
    public string ConnectionString { get; init; }
}