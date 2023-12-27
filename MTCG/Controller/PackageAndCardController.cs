using Microsoft.Extensions.Logging;
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
    public class PackageAndCardController : BaseController
    {

        private IPackageAndCardService _packageService;
        private ISessionService _sessionService;
        public PackageAndCardController(IPackageAndCardService packageAndCardService, ISessionService sessionService)
        {
            _packageService = packageAndCardService ?? throw new ArgumentNullException(nameof(packageAndCardService));
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }


        public override void HandleRequest(HttpSvrEventArgs e)
        {
            if (e.Method == "POST")
            {
                if (e.Path.StartsWith("/packages"))
                {
                    CreateNewCardPackage(e);

                }
                if (e.Path.StartsWith("/transactions/packages"))
                {
                    BuyCardPackage(e);
                }
            }
        }

        public void CreateNewCardPackage(HttpSvrEventArgs e)
        {

            Console.WriteLine("in create newcardpackage controller");
            switch (_packageService.CreateNewPackage(e))
            {
                case ("success"):
                    Console.WriteLine("Package and crrds succes created");

                    e.Reply((int)HttpCodes.OK, "{\"description\":\"Package and cards successfully created.\"}");

                    break;
                case ("token"):
                    e.Reply((int)HttpCodes.INTERNAL_SERVER_ERROR, "{\"msg\":\"User could not be created. Something went wrong\"}");

                    //e.Reply((int)HttpCodes.UNAUTORIZED,"{\"msg\":\"Access token is missing or invalid.\"}");
                    break;
                case ("admin"):
                    e.Reply((int)HttpCodes.FORBIDDEN, "{\"description\":\"Provided user is not admin.\"}");
                    break;
                case ("409"):
                    e.Reply((int)HttpCodes.CONFLICT, "{\"description\":\"At least one card in the packages already exists\"}");
                    break;
            }
        }
        public void BuyCardPackage(HttpSvrEventArgs e)
        {
            //                    e.Reply((int)HttpCodes.BAD_REQUEST, "{\"description\":\"User could not be logged in - got exception.\"}");

            e.Reply((int)HttpCodes.OK, "{\"description\":\"A package has been successfully bought.\"}");
            e.Reply((int)HttpCodes.FORBIDDEN, "{\"description\":\"Not enough money for buying a card package.\"}");
            e.Reply((int)HttpCodes.NOT_FOUND, "{\"description\":\" No card package available for buying.\"}");
        }


    }
}
