using System.Text.Json.Serialization;


namespace TestProject.DataModel
{
    internal class RequestVariables
    {
        [JsonPropertyName("size")]
        public int Size { get; } = Constants.PAGE_SIZE;

        [JsonPropertyName("number")]
        public int Number { get; set; } = 0;

        [JsonPropertyName("filter")]
        public string Filter { get; set; } = null;

        [JsonPropertyName("orders")]
        public string Orders { get; set; } = null;
    }
}