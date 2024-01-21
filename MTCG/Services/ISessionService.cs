using MTCG.HttpServer;
using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Services
{
    public interface ISessionService


    {
        public string AuthenticateAndCreateSession(UserCredentials userCredentials, TcpClient client);
       
        public Guid AuthenticateUserAndSession(HttpSvrEventArgs e, string? username);

        public TcpClient? GetClientFromSession(Guid userId);
    }
}
