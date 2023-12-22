using MTCGServer.BusinessLogic.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCGServer.Network.HTTP;
using System.Text.Json;
using MTCGServer.Models.SerializationObjects;
using MTCGServer.Models.DataModels;
using MTCGServer.BusinessLogic.Exceptions;
using MTCGServer.DataAccess.UnitOfWork;
using Npgsql;

namespace MTCGServer.BusinessLogic.EndPointHandlers
{
    [EndPointHandler]
    internal class CardHandler
    {
        // private variables
        private readonly string _connectionString;

        // constructor
        public CardHandler(string connectionstring)
        {
            _connectionString = connectionstring;
        }

        [EndPoint(HttpMethods.GET, "/cards")]
        public HttpResponse GetCards(HttpRequest request)
        {
            try
            {
                // create UnitOfWork
                using UnitOfWork unit = new(_connectionString, withTransaction: false);                

                // get user from token and check permissions
                if (unit.UserRepository.GetUser(request.Token) is not User user)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");

                // get cards from user
                List<Card> toReturn = unit.CardRepository.GetCards(user.Id);

                if (toReturn.Count == 0)
                    return new HttpResponse("204 No Content");

                return new HttpResponse("200 OK", JsonSerializer.Serialize(toReturn));
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
        }
    }

}
