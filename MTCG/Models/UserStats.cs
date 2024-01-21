using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace MTCG.Models
{
    public class UserStats
    {
        /// <summary>This class provides the model for the user statistics.</summary>


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [JsonIgnore]
        public Guid Id { get; set; }    
        public string Name { get; set; } = string.Empty;
        public int Elo { get; set; } = 1000;
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
    
    }

}
