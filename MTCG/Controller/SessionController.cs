using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Controller
{
    public class SessionController
    {
        private ISessionService _sessionService;
        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }

        public void AuthenticateAndCreateSession(UserCredentials? userCredentials, HttpSvrEventArgs e)
        {
            Console.WriteLine("in controller authenticate");

            try
            {
                string token = _sessionService.AuthenticateAndCreateSession(userCredentials);
                Console.WriteLine("got token back in sessionscontroller" + token);
                if (!(string.IsNullOrEmpty(token)))
                {
                    string response = "{\"msg\":\"User was logged in with token: " + token + "\"}";
                    e.Reply((int)HttpCodes.OK, response);
                }
                else
                {
                    e.Reply((int)HttpCodes.INTERNAL_SERVER_ERROR, "{\"msg\":\"User could not logged in. Something went wrong\"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                e.Reply((int)HttpCodes.BAD_REQUEST, "{\"msg\":\"User could not be logged ig - got exception.\"}");
            }
        }
    }
}
