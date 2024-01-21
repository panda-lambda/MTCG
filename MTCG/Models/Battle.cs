using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
        public required Player PlayerOne { set; get; }
        public required Player PlayerTwo { set; get; }
        public List<string> LogPlayerOne { get; set; } = new();
        public List<string> LogPlayerTwo { get; set; } = new();
         
        public int Rounds { set; get; } = 0;
        public ResultType? Result { set; get; }





    }
}
