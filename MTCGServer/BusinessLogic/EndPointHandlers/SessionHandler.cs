using MTCGServer.BusinessLogic.Attributes;
using MTCGServer.BusinessLogic.Exceptions;
using MTCGServer.BusinessLogic.Managers;
using MTCGServer.Models.SerializationObjects;
using MTCGServer.Models.UserModels;
using MTCGServer.Network.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MTCGServer.BusinessLogic.EndPointHandlers
{
    [EndPointHandler]
    public class SessionHandler
    {
        // private variables
        private readonly TokenManager _tokenManager;
        private readonly UserManager _userManager;
        private readonly CardManager _cardManager;

        // constructor
        public SessionHandler(UserManager userManager, TokenManager tokenManager, CardManager cardManager)
        {
            _tokenManager = tokenManager;
            _userManager = userManager;
            _cardManager = cardManager;
        }

        // public methods
        [EndPoint(HttpMethods.POST, "/sessions")]
        public HttpResponse PostSession(HttpRequest request)
        {
            try
            {
                // deserialize body
                UserCredentials? userCredentials = JsonSerializer.Deserialize<UserCredentials>(request.Body);

                // check if body is valid
                if (userCredentials == null ||
                   userCredentials.Username == null ||
                   userCredentials.Password == null)
                    throw new HttpException("400 Bad Request", "Invalid Body");

                // get user from database
                if (_userManager.GetUser(userCredentials.Username, userCredentials.Password) is not User user)
                    throw new HttpException("401 Unauthorized", "Invalid username/password provided");
                    
                // generate token and create session
                string token = _tokenManager.GenerateTokenForUser(user.Id);

                // send token to client
                AuthToken authToken = new()
                {
                    Token = token
                };
                return new HttpResponse("200 OK", JsonSerializer.Serialize(authToken));
            }
            catch (JsonException)
            {
                Error error = new()
                {
                    ErrorMessage = "Invalid body"
                };
                return new HttpResponse("400 Bad Request", JsonSerializer.Serialize(error));
            }
            catch (HttpException ex)
            {
                return new HttpResponse(ex.Header, JsonSerializer.Serialize(ex.Error));
            }
        }
    }
}
