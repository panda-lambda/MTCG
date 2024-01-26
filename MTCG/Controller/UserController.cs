using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Services;
using MTCG.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MTCG.Controller
{
    public class UserController : BaseController
    {
        private IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }


        public override void HandleRequest(HttpSvrEventArgs e)
        {
            switch (e.Method)
            {
                case "POST":
                    if (e.Path.StartsWith("/users"))
                        ExecuteWithExceptionHandling(e, CreateUser);
                    break;
                case "PUT":
                    if (e.Path.StartsWith("/users/"))
                    {
                        ExecuteWithExceptionHandling(e, UpdateUserData);
                    }
                    break;
                case "GET":
                    if (e.Path.StartsWith("/users/"))
                    {
                        ExecuteWithExceptionHandling(e, GetUserData);
                    }
                    break;
                default:
                    e.Reply((int)HttpCodes.BAD_REQUEST, "{\"description\":\"Not a valid Http Request!\"}");
                    break;
            }
        }



        internal void CreateUser(HttpSvrEventArgs e)
        {
            Console.WriteLine("in create use usercontroller");
            UserCredentials? userCredentials = JsonSerializer.Deserialize<UserCredentials>(e.Payload);

            if (userCredentials == null || string.IsNullOrEmpty(userCredentials.Username) || string.IsNullOrEmpty(userCredentials?.Password))
            {
                e.Reply((int)HttpCodes.BAD_REQUEST, "{\"description\":\"User could not be created. No valid registration data\"}");
                return;
            }

            if (_userService.CreateUser(userCredentials.Username, userCredentials.Password))
            {
                e.Reply((int)HttpCodes.CREATED, "{\"description\":\"User successfully created\"}");
            }
            else
            {
                e.Reply((int)HttpCodes.INTERNAL_SERVER_ERROR, "{\"description\":\"User could not be created. Something went wrong\"}");
            }
    
        }

        internal void GetUserData(HttpSvrEventArgs e)
        {

            UserData? userData = _userService.GetUserData(e);
            if (userData == null)
            {
                throw new UserNotFoundException("User not found");
            }

            e.Reply((int)HttpCodes.OK, System.Text.Json.JsonSerializer.Serialize(userData,JsonOptions.NullOptions));
        }


        internal void UpdateUserData(HttpSvrEventArgs e)
        {

            if (_userService.UpdateUserData(e))
            {
                e.Reply((int)HttpCodes.OK, "{\"msg\":\"User sucessfully updated.\"}");
            }

        }

    }
}
