using Newtonsoft.Json;

namespace Domain.Aggregates
{
    public interface IAggregrateRoot
    {
        [JsonProperty("id")]
        public int Id { get; }
    }
}
