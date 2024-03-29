﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class UserCredentials
    {
        /// <summary>This class provides the model for the user credentials.</summary>

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public Guid? Id { get; set; }
    }
}
