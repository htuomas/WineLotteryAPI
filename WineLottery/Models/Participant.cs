using Newtonsoft.Json;

namespace WineLottery.Models
{
    public class Participant
    {
        [JsonIgnore]
        public string Id { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public string DraftId { get; set; }
        [JsonIgnore]
        public bool HasWon { get; set; }
    }
}