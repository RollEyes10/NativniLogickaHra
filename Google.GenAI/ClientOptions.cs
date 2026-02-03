namespace Google.GenAI
{
    // Minimal shim to provide ClientOptions when the official library isn't referenced.
    // Only contains the properties used in this project.
    public class ClientOptions
    {
        public string? ApiKey { get; set; }
    }
}
