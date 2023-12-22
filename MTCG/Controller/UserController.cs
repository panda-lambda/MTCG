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
    public class UserController
    {
        private IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public void CreateUser(UserCredentials? userCredentials, HttpSvrEventArgs e)
        {
            Console.WriteLine(userCredentials.Username, userCredentials.Password);
            Console.WriteLine("in ceate use usercontroller");

            try

            {
                if (userCredentials == null)
                {
                    e.Reply((int)HttpCodes.BAD_REQUEST, "{\"msg\":\"User could not be created. No valid credentials\"}");
                }

                if (_userService.CreateUser(userCredentials.Username, userCredentials.Password))
                {
                    e.Reply((int)HttpCodes.OK, "{\"msg\":\"User was created.\"}");
                }
                else
                {
                    e.Reply((int)HttpCodes.INTERNAL_SERVER_ERROR, "{\"msg\":\"User could not be created. Something went wrong\"}");
                }
            }
            catch (Exception)
            {
                e.Reply((int)HttpCodes.BAD_REQUEST, "{\"msg\":\"User could not be created - got exception.\"}");

            }

            return;
        }


    }
}
