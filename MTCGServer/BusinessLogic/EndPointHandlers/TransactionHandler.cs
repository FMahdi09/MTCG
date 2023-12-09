using MTCGServer.BusinessLogic.Attributes;
using MTCGServer.BusinessLogic.Exceptions;
using MTCGServer.BusinessLogic.Managers;
using MTCGServer.Models.DataModels;
using MTCGServer.Models.SerializationObjects;
using MTCGServer.Models.UserModels;
using MTCGServer.Network.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MTCGServer.Businesslogic.EndPointHandlers
{
    [EndPointHandler]
    internal class TransactionHandler
    {
        // public variables
        public const int PackagePrice = 5;
        public const int CardsInPackage = 5;

        // private variables
        private readonly TokenManager _tokenManager;
        private readonly UserManager _userManager;
        private readonly CardManager _cardManager;

        // constructor
        public TransactionHandler(UserManager userManager, TokenManager tokenManager, CardManager cardManager) 
        { 
            _userManager = userManager;
            _tokenManager = tokenManager;
            _cardManager = cardManager;
        }

        [EndPoint(HttpMethods.POST, "/transactions/packages")]
        public HttpResponse PostPackage(HttpRequest request) 
        {
            try
            {
                // get user from token and check permissions
                if (_userManager.GetUser(request.Token) is not User user)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");

                // check if user is able to pay
                if (!_userManager.PayCurrency(user.Id, PackagePrice))
                    throw new HttpException("403 Forbidden", "Not enough money for buying a card package");

                // generate cards for user
                List<Card> generatedCards = _cardManager.GenerateCards(CardsInPackage);

                // assign cards to user
                if (!_cardManager.AssignCards(user.Id, generatedCards))
                    throw new HttpException("500 Internal Server Error", "There was an error generating your cards (this should not happen)");                

                return new HttpResponse("200 OK", JsonSerializer.Serialize(generatedCards));
            }
            catch(HttpException ex)
            {
                return new HttpResponse(ex.Header, JsonSerializer.Serialize(ex.Error));
            }
        }
    }
}
