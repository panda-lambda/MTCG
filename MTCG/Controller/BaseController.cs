using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Controller
{
    public abstract class BaseController
    {
        /// <summary>
        /// Catches exceptions and replies with the appropriate status code and message.
        /// </summary>
        /// <param name="e">HttpServer Event Args</param>
        /// 

        
        public abstract void HandleRequest(HttpSvrEventArgs e);

        /// <summary>
        /// uses a delegate to execute the action and catches exceptions.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="action"></param>

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
           

            catch (UserCurrentlyFightingException)
            {
                e.Reply((int)HttpCodes.CONFLICT, "{\"description\":\"User is currently fighting, please wait for the fight to finish.\"}");
            }
                  

            catch (ConflictException ex)
            {
                e.Reply((int)HttpCodes.CONFLICT, $"{{\"description\":\"{ex.Message}\"}}");
            }

            catch (NoContentException ex)
            {
                e.Reply((int)HttpCodes.NO_CONTENT, $"{{\"description\":\"{ex.Message}\"}}");
            }

            catch (ForbiddenException ex)
            {
                e.Reply((int)HttpCodes.FORBIDDEN, $"{{\"description\":\"{ex.Message}\"}}");
            }

            catch (NotFoundException ex)
            {
                e.Reply((int)HttpCodes.NOT_FOUND, $"{{\"description\":\"{ex.Message}\"}}");
            }
            catch (BadRequestException ex)
            {
                e.Reply((int)HttpCodes.NOT_FOUND, $"{{\"description\":\"{ex.Message}\"}}");
             }

            catch (Exception ex)
            {
                Console.WriteLine(ex);
                e.Reply((int)HttpCodes.INTERNAL_SERVER_ERROR, $"{{\"description\":\"{ex.Message}\"}}");
            }


        }
    }
}
