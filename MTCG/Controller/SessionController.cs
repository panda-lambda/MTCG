using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using static System.Collections.Specialized.BitVector32;
using System.Xml;

namespace MTCG.Controller
{
    public class SessionController : BaseController
    {
        private ISessionService _sessionService;
        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }

        public override void HandleRequest(HttpSvrEventArgs e)
        {
            ExecuteWithExceptionHandling(e, AuthenticateAndCreateSession);
        }

        public void AuthenticateAndCreateSession(HttpSvrEventArgs e)
        {
            //Console.WriteLine("in controller authenticate");
            UserCredentials? userCredentials = JsonSerializer.Deserialize<UserCredentials>(e.Payload);
            if (userCredentials != null)
            {
                try
                {
                    if(e.Client == null)
                    {
                        Console.WriteLine("client is null in controller auth and create session");
                    }
                    else
                    {
                        Console.WriteLine("client is NOT null in controller auth and create session");
                    }
                    string token = _sessionService.AuthenticateAndCreateSession(userCredentials, e.Client);
                    Console.WriteLine("got token back in sessionscontroller:  \n" + token);
                    if (!(string.IsNullOrEmpty(token)))
                    {
                        e.Reply((int)HttpCodes.OK, $"{{ \"token\": \"{token}\" }}");
                    }
                    else
                    {
                        e.Reply((int)HttpCodes.UNAUTORIZED, "{\"description\":\"Access token is missing or invalid\"}");
                    }


                
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    e.Reply((int)HttpCodes.UNAUTORIZED, "{\"description\":\"Invalid username/password provided.\"}");
                }
            }
        }

      

    }

}
