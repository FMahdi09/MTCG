using MTCGServer.BusinessLogic.Attributes;
using MTCGServer.BusinessLogic.Exceptions;
using MTCGServer.DataAccess.UnitOfWork;
using MTCGServer.Models.DataModels;
using MTCGServer.Models.SerializationObjects;
using MTCGServer.Network.HTTP;
using Npgsql;
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
        private readonly string _connectionString;

        // constructor
        public SessionHandler(string connectionstring)
        {
            _connectionString = connectionstring;
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
                if (userCredentials == null          ||
                    userCredentials.Username == null ||
                    userCredentials.Password == null)
                    throw new HttpException("400 Bad Request", "Invalid Body");

                // create UnitOfWork
                using UnitOfWork unit = new(_connectionString, withTransaction: false);

                // get user from credentials
                if (unit.UserRepository.GetUser(userCredentials.Username, userCredentials.Password) is not User user)
                    throw new HttpException("401 Unauthorized", "Invalid username/password provided");
                    
                // generate token and create session
                string token = Guid.NewGuid().ToString();

                unit.TokenRepository.AddToken(user.Id, token);

                // send token to client
                return new HttpResponse("200 OK", JsonSerializer.Serialize(new AuthToken(token)));
            }
            catch (HttpException ex)
            {
                return new HttpResponse(ex.Header, JsonSerializer.Serialize(ex.Error));
            }
            catch (PostgresException)
            {
                return new HttpResponse("500 Internal Server Error");
            }
            catch (NpgsqlException)
            {
                return new HttpResponse("500 Internal Server Error", JsonSerializer.Serialize(new Error("Unable to connect to database")));
            }
            catch (JsonException)
            {
                return new HttpResponse("400 Bad Request", JsonSerializer.Serialize(new Error("Invalid Body")));
            }
        }
    }
}
