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
        string AuthenticateAndCreateSession(UserCredentials userCredentials, TcpClient client);
       
        Guid AuthenticateUserAndSession(HttpSvrEventArgs e, string? username);

        TcpClient? GetClientFromSession(Guid userId);
        void SetFightingState(Guid userId, bool isFighting);

    }
}
