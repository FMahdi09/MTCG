using MTCGServer.BusinessLogic.Managers;
using MTCGServer.Network.HTTP;
using MTCGServer.BusinessLogic.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MTCGServer.Models.SerializationObjects;
using MTCGServer.Models.UserModels;
using MTCGServer.BusinessLogic.Exceptions;

namespace MTCGServer.BusinessLogic.EndPointHandlers
{
    [EndPointHandler]
    public class UserHandler
    {
        // private variables
        private readonly UserManager _userManager;
        private readonly TokenManager _tokenManager;
        private readonly CardManager _cardManager;

        // constructor
        public UserHandler(UserManager userManager, TokenManager tokenManager, CardManager cardManager)
        {
            _userManager = userManager;
            _tokenManager = tokenManager;
            _cardManager = cardManager;
        }

        // public methods        
        [EndPoint(HttpMethods.POST, "/users")]
        public HttpResponse PostUser(HttpRequest request)
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

                // add user if username is not taken
                if (!_userManager.AddUser(userCredentials.Username, userCredentials.Password))
                    throw new HttpException("409 Conflict", "Username already exists");

                return new HttpResponse("201 Created");
            }
            catch (JsonException)
            {
                Error error = new()
                {
                    ErrorMessage = "Invalid Body"
                };
                return new HttpResponse("400 Bad Request", JsonSerializer.Serialize(error));
            }
            catch (HttpException ex)
            {
                return new HttpResponse(ex.Header, JsonSerializer.Serialize(ex.Error));
            }
        }

        [EndPoint(HttpMethods.GET, "/users/{username}")]
        public HttpResponse GetUser(HttpRequest request)
        {
            try
            {
                // get username from path
                string username = request.Resource[1];

                // get user from token and check permissions
                if (_userManager.GetUser(request.Token) is not User user ||
                   username != user.Username)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");

                // send userdata to client
                UserData userData = new()
                {
                    Username = user.Username,
                    Bio = user.Bio,
                    Image = user.Image
                };
                return new HttpResponse("200 OK", JsonSerializer.Serialize(userData));
            }
            catch (HttpException ex)
            {
                return new HttpResponse(ex.Header, JsonSerializer.Serialize(ex.Error));
            }
        }

        [EndPoint(HttpMethods.PUT, "/users/{username}")]
        public HttpResponse PutUser(HttpRequest request)
        {
            // get username from path
            string username = request.Resource[1];

            try
            {
                // deserialize body
                UserData? userData = JsonSerializer.Deserialize<UserData>(request.Body);

                // check if body is valid
                if (userData == null ||
                   userData.Username == null ||
                   userData.Image == null ||
                   userData.Bio == null)
                    throw new HttpException("400 Bad Request", "Invalid Body");

                // get user from token and check permissions
                if (_userManager.GetUser(request.Token) is not User user ||
                    username != user.Username)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");

                // update user
                user.Username = userData.Username;
                user.Image = userData.Image;
                user.Bio = userData.Bio;

                // enter user in db
                if (!_userManager.UpdateUser(user))
                    throw new HttpException("409 Conflict", "Username already exists");

                return new HttpResponse("200 OK");            
            }
            catch(JsonException)
            {
                Error error = new()
                {
                    ErrorMessage = "Invalid Body"
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
