using MTCG.HttpServer;
using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MTCG.Controller
{
    public class GameController : BaseController
    {
        public override void HandleRequest(HttpSvrEventArgs e)
        {
            if (e.Path.StartsWith("/stats") && e.Method == "GET")
            {
                GetStatsByUser(e);
            }

            if (e.Path.StartsWith ("/scoreboard") && e.Method == "GET") {
                GetSortedStatsByELO(e);
            }
            if (e.Path.StartsWith("/battles") && e.Method == "POST")
            {

                e.Reply((int)HttpCodes.OK, "the battle log");
                e.Reply((int)HttpCodes.UNAUTORIZED, "{\"description\":\"Access token is missing or invalid\"}");

            }
        }

      

        private void GetStatsByUser(HttpSvrEventArgs e)
        {
            
            e.Reply((int)HttpCodes.OK, "content");
        //description: The stats could be retrieved successfully.
        //  content:
        //    application / json:
        //      schema:
        //        $ref: '#/components/schemas/UserStats'
            e.Reply((int)HttpCodes.UNAUTORIZED, "{\"description\":\"Access token is missing or invalid\"}");

        }
        private void GetSortedStatsByELO(HttpSvrEventArgs e)
        {
            e.Reply((int)HttpCodes.OK, "content");

            //    '200':
            //  description: The scoreboard could be retrieved successfully.
            //  content:
            //    application / json:
            //      schema:
            //type: array
            //items:
            //          $ref: '#/components/schemas/UserStats'
        }
    }
}
