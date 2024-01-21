using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Services
{
    public interface ISessionServiceWithSessions:ISessionService
    {
        public UserSession GetSession(Guid sessionId);
    }
}
