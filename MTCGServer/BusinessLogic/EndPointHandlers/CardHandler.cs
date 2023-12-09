using MTCGServer.BusinessLogic.Managers;
using MTCGServer.BusinessLogic.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCGServer.Network.HTTP;
using MTCGServer.Models.UserModels;
using System.Text.Json;
using MTCGServer.Models.SerializationObjects;
using MTCGServer.Models.DataModels;
using MTCGServer.BusinessLogic.Exceptions;

namespace MTCGServer.BusinessLogic.EndPointHandlers
{
    [EndPointHandler]
    internal class CardHandler
    {
        // private variables
        private readonly TokenManager _tokenManager;
        private readonly UserManager _userManager;
        private readonly CardManager _cardManager;

        // constructor
        public CardHandler(UserManager userManager, TokenManager tokenManager, CardManager cardManager)
        {
            _tokenManager = tokenManager;
            _userManager = userManager;
            _cardManager = cardManager;
        }

        [EndPoint(HttpMethods.GET, "/cards")]
        public HttpResponse GetCards(HttpRequest request)
        {
            try
            {
                // get user from token and check permissions
                if (_userManager.GetUser(request.Token) is not User user)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");                

                // get cards from user
                List<Card> toReturn = _cardManager.GetCardsForUser(user.Id);

                if (toReturn.Count == 0)
                    return new HttpResponse("204 No Content");

                return new HttpResponse("200 OK", JsonSerializer.Serialize(toReturn));
            }
            catch(HttpException ex)
            {
                return new HttpResponse(ex.Header, JsonSerializer.Serialize(ex.Error));
            }
        }
    }
}
