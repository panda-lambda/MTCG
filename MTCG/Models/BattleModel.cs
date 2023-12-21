using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class BattleModel
    {
        /// <summary>This class provides the model for the battles.</summary>

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Guid BattleID { get; set; }
        public Guid? FirstPlayer { set; get; }
        public Guid? SecondPlayer { set; get; }
        public int Rounds { set; get; } = 0;
        public ResultType? Result { set; get; }





    }
}
