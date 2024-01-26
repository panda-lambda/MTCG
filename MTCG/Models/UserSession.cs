using System.Net.Sockets;

namespace MTCG.Models
{
    public class UserSession
    {
        /// <summary>This class provides the model for a user session.</summary>


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public long CreatedAt { get; set; }
        public long ExpiresAt { get; set; }
        public bool IsFighting { get; set; } = false;
        public TcpClient? TCPClient { get; set; }



    }



}


