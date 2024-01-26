using System.Text.Json.Serialization;

namespace MTCG.Models
{
    public class UserData
    {

        /// <summary>This class provides the model for the user data.</summary>

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [JsonIgnore]
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public string? Image { get; set; }

        [JsonIgnore]
        public int Coins { get; set; } = 20;
    }

}
