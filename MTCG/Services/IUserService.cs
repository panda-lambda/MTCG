using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Services
{
    public interface IUserService
    {
        internal bool CreateUser(string username, string password);
        public bool VerifyPassword(string inputPassword, string hash);



    }
}
