using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class Battle
    {
        /// <summary>This class provides the model for the battles.</summary>

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Guid BattleID { get; set; }
        public Guid? PlayerOne { set; get; }
        public Guid? PlayerTwo { set; get; }
        public List<Card>? DeckPlayerOne { get; set; }
        public List<Card>? DeckPlayerTwo { get; set; }
        public List<string>? LogPlayerOne { get; set; }
        public List<string>? LogPlayerTwo { get; set; }

        public int Rounds { set; get; } = 0;
        public ResultType? Result { set; get; }





    }
}
