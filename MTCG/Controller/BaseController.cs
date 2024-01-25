using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Controller
{
    public abstract class BaseController
    {
        public abstract void HandleRequest(HttpSvrEventArgs e);

        protected static void ExecuteWithExceptionHandling(HttpSvrEventArgs e, Action<HttpSvrEventArgs> action)
        {
            try
            {
                action(e);
            }
            catch (UnauthorizedException)
            {
                e.Reply((int)HttpCodes.UNAUTORIZED, "{\"description\":\"Access token is missing or invalid.\"}");
            }

            catch (InvalidCardCountInDeck)
            {
                e.Reply((int)HttpCodes.BAD_REQUEST, "{\"description\":\"The provided deck did not include the required amount of cards.\"}");
            }

            catch (UserNotCardOwnerException ex)
            {
                Console.WriteLine("ex dort: " + ex.Message);

                e.Reply((int)HttpCodes.FORBIDDEN, "{\"description\":\"At least one of the provided cards does not belong to the user or is not available..\"}");

            }

            catch (UserHasNoValidDeckException)
            {
                e.Reply((int)HttpCodes.CONFLICT, "{\"description\":\"User has no valid deck.\"}");
            }

            catch (UserCurrentlyFightingException)
            {
                e.Reply((int)HttpCodes.CONFLICT, "{\"description\":\"User is currently fighting, please wait for the fight to finish.\"}");
            }

            catch (InvalidCardForDealException)
            {
                e.Reply((int)HttpCodes.FORBIDDEN, "{\"description\":\"The deal contains a card that is not owned by the user or locked in the deck.\"}");
            }

            catch (DealAlreadyExistsException)
            {
                e.Reply((int)HttpCodes.CONFLICT, "{\"description\":\"A deal with this deal ID already exists.\"}");
            }

            catch (NoAvailableTradingDealsException)
            {
                e.Reply((int)HttpCodes.NO_CONTENT, "{\"description\":\"The request was fine, but there are no trading deals available.\"}");
            }

            catch (ForbiddenException ex)
            {
                e.Reply((int)HttpCodes.FORBIDDEN, $"{{\"description\":\"{ex}\"}}");
            }

            catch (NotFoundException ex)
            {
                e.Reply((int)HttpCodes.NOT_FOUND, $"{{\"description\":\"{ex}\"}}");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                e.Reply((int)HttpCodes.INTERNAL_SERVER_ERROR, "{\"msg\":\"Something went wrong.\"}");
            }




        }
    }
}
