using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Services
{
    public interface ISessionService


    {
        public string AuthenticateAndCreateSession(UserCredentials userCredentials);
        public bool ValidateToken(string token);

    }
}
