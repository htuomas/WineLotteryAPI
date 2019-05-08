using Newtonsoft.Json;

namespace WineLottery.Models
{
    public class Participant
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public string DraftId { get; set; }
        public bool HasWon { get; set; }
    }
}