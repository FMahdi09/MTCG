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
    internal class DeckHandler
    {
        // constants
        const int DeckSize = 4;

        // private variables
        private readonly string _connectionString;

        // constructor
        public DeckHandler(string connectionString)
        {
            _connectionString = connectionString;
        }

        [EndPoint(HttpMethods.GET, "/deck")]
        public HttpResponse GetDeck(HttpRequest request)
        {
            try
            {
                // create UnitOfWork
                using UnitOfWork unit = new(_connectionString, withTransaction: false);

                // get user from token and check permissions
                if(unit.UserRepository.GetUser(request.Token) is not User user)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");

                // get deck from user
                List<Card> deck = unit.DeckRepository.GetDeck(user.Id);

                if (deck.Count == 0)
                    return new HttpResponse("204 No Content");

                return new HttpResponse("200 OK", JsonSerializer.Serialize(deck));
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

        [EndPoint(HttpMethods.PUT, "/deck")]
        public HttpResponse PutDeck(HttpRequest request) 
        { 
            try
            {
                // deserialize body
                List<string> cardIds = JsonSerializer.Deserialize<List<string>>(request.Body)
                    ?? throw new HttpException("400 Bad Request", "Invalid Body");

                // remove all duplicates
                cardIds = cardIds.Distinct().ToList();

                // check if correct number of cards have been provided
                if (cardIds.Count != DeckSize)
                    throw new HttpException("400 Bad Request", "Invalid number of cards");

                // create UnitOfWork
                using UnitOfWork unit = new(_connectionString, withTransaction: true);

                // get user from token and check permissions
                if (unit.UserRepository.GetUser(request.Token) is not User user)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");

                // check card ownership
                if(!unit.CardRepository.CheckOwnership(user.Id, cardIds))
                    throw new HttpException("403 Forbidden", "Invalid cards provided");

                // clear deck
                unit.DeckRepository.ClearDeck(user.Id);

                // update new deck
                unit.DeckRepository.UpdateDeck(user.Id, cardIds);

                // commit work
                unit.Commit();

                return new HttpResponse("200 OK");
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
