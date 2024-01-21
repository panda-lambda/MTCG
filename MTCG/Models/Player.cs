using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class Player
    {
        /// <summary>This class provides the model for the battles.</summary>

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///

        public Guid Id { get; set; }

        public required string Name { get; set; }
        public required  UserStats Stats { get; set; }  
        public required Deck Deck { get; set; }      
        
        public required TcpClient Client { get; set; }

    }
}
