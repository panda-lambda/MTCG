using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class TradingDeal
    {
        /// <summary>This class provides the model for a card.</summary>

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        

        public Guid Id { get; set; }
        [JsonIgnore]
        public Guid OwnerId { get; set; }
        public Guid CardToTrade { get; set; }
        public CardType? Type { get; set; } 
        public float? MinDamage { get; set; }

       
    }
}
