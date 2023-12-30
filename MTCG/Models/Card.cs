using System.Text.Json.Serialization;

namespace MTCG.Models
{
    public class Card
    {
        /// <summary>This class provides the model for a card.</summary>

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Guid Id { get; set; }
        public FactionType Name { get; set; }
        public float Damage { get; set; }

        [JsonIgnore]
        public CardType Type { get; set; }

        [JsonIgnore]
        public string? Description { set; get; }

        [JsonIgnore]
        public ElementType Element { get; set; }

    }
}