using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public  class RoundResult
    {
        /// <summary>This class provides the model for a battle round result.</summary>

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public ResultType? Result { get; set;  }
        public string? LogRoundPlayerOne { get; set; }
        public string? LogRoundPlayerTwo { get; set; }

    } 
}
