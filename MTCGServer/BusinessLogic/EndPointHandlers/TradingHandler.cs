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
    internal class TradingHandler
    {
        // private variables
        private readonly string _connectionString;

        // constructor
        public TradingHandler(string connectionString)
        {
            _connectionString = connectionString;
        }

        [EndPoint(HttpMethods.POST, "/tradings")]
        public HttpResponse PostTradingdeal(HttpRequest request) 
        { 
            try
            {
                // deserialize body
                TradeOffer? offer = JsonSerializer.Deserialize<TradeOffer>(request.Body);

                // check if body is valid
                if(offer == null ||
                   offer.CardType == null ||
                   offer.Cardtoken == null ||
                   offer.MinDamage == null)
                    throw new HttpException("400 Bad Request", "Invalid Body");

                // create UnitOfWork
                using UnitOfWork unit = new(_connectionString, withTransaction: true);

                // get user from token and check permissions
                if (unit.UserRepository.GetUser(request.Token) is not User user)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");

                // get card from token
                if(unit.CardRepository.GetCard(user.Id, offer.Cardtoken) is not Card card)
                    throw new HttpException("403 Forbidden", "Invalid card provided");

                // check if card is in deck
                if (unit.DeckRepository.IsInDeck(user.Id, card))
                    throw new HttpException("403 Forbidden", "Card is in deck");

                // create deal
                Tradingdeal deal = new(tradeId: Guid.NewGuid().ToString(), 
                                       userId: user.Id, 
                                       card: card, 
                                       minDamage: offer.MinDamage.GetValueOrDefault(), 
                                       cardType: offer.CardType);

                unit.TradingRepository.CreateDeal(deal);
                
                // unassign card from user
                unit.CardRepository.UnassignCard(user.Id, offer.Cardtoken);

                // commit work
                unit.Commit();

                return new HttpResponse("201 Created");
            }
            catch (HttpException ex)
            {
                return new HttpResponse(ex.Header, JsonSerializer.Serialize(ex.Error));
            }
            catch (PostgresException ex)
            {
                return ex.SqlState switch
                {
                    // 23502: not null violation
                    "23502" => new HttpResponse("400 Bad Request", JsonSerializer.Serialize(new Error("Invalid tradingdetails provided"))),

                    // default
                    _ => new HttpResponse("500 Internal Server Error")
                };
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

        [EndPoint(HttpMethods.GET, "/tradings")]
        public HttpResponse GetAllTradingdeals(HttpRequest request)
        {
            try
            {
                // create UnitOfWork
                using UnitOfWork unit = new(_connectionString, withTransaction: false);

                // get user from token and check permissions
                if (unit.UserRepository.GetUser(request.Token) is not User user)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");

                // get trading deals
                List<Tradingdeal> deals = unit.TradingRepository.GetDeals();

                if (deals.Count == 0)
                    return new HttpResponse("204 No Content");

                return new HttpResponse("200 OK", JsonSerializer.Serialize(deals));
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

        [EndPoint(HttpMethods.GET, "/tradings/{username}")]
        public HttpResponse GetUserTradingdeals(HttpRequest request) 
        { 
            try
            {
                // get username from resource
                string username = request.Resource[1];

                // create UnitOfWork
                using UnitOfWork unit = new(_connectionString, withTransaction: false);

                // get user from token and check permissions
                if (unit.UserRepository.GetUser(request.Token) is not User user ||
                   user.Username != username)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");

                // get trading deals
                List<Tradingdeal> deals = unit.TradingRepository.GetDeals(user.Id);

                if (deals.Count == 0)
                    return new HttpResponse("204 No Content");

                return new HttpResponse("200 OK", JsonSerializer.Serialize(deals));
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

        [EndPoint(HttpMethods.DELETE, "/tradings/{trading_token}")]
        public HttpResponse DeleteTradingdeal(HttpRequest request) 
        { 
            try
            {
                // get deal token from resource
                string dealToken = request.Resource[1];

                // create UnitOfWork
                using UnitOfWork unit = new(_connectionString, withTransaction: true);

                // get user from token and check permissions
                if(unit.UserRepository.GetUser(request.Token) is not User user)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");

                // get deal
                if (unit.TradingRepository.GetDeal(dealToken) is not Tradingdeal deal)
                    throw new HttpException("404 Not Found", "Unable to find deal with provided token");

                // check if deal belongs to user
                if (deal.UserId != user.Id)
                    throw new HttpException("403 Forbidden", "Not allowed to acces this deal");

                // delete deal
                unit.TradingRepository.DeleteDeal(deal);

                // assign card back to owner
                unit.CardRepository.ReassignCard(user.Id, deal.Card);

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
        }

        [EndPoint(HttpMethods.POST, "/tradings/{trading_token}")]
        public HttpResponse AcceptTraidingdeal(HttpRequest request)
        {
            try
            {
                // get deal token from resource
                string dealToken = request.Resource[1];

                // deserialize body
                string cardToken = JsonSerializer.Deserialize<string>(request.Body) 
                    ?? throw new HttpException("400 Bad Request", "Invalid Body");

                // create UnitOfWork
                using UnitOfWork unit = new(_connectionString, withTransaction: true);

                // get user from token and check permissions
                if (unit.UserRepository.GetUser(request.Token) is not User user)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");

                // get card from provided token
                if(unit.CardRepository.GetCard(user.Id, cardToken) is not Card card)
                    throw new HttpException("403 Forbidden", "Invalid card provided");

                // check if card is in deck
                if(unit.DeckRepository.IsInDeck(user.Id, card))
                    throw new HttpException("403 Forbidden", "Card is in deck");

                // get deal from provided token
                if (unit.TradingRepository.GetDeal(dealToken) is not Tradingdeal deal)
                    throw new HttpException("404 Not Found", "Unable to find deal with provided token");

                // check trading requirements
                if(card.CardType != deal.CardType ||
                   card.Damage < deal.MinDamage)
                    throw new HttpException("403 Forbidden", "card does not match requirements");

                // perform trade:

                // reassign cards
                unit.CardRepository.ReassignCard(user.Id, deal.Card);
                unit.CardRepository.ReassignCard(deal.UserId, card);

                // delete tradedeal
                unit.TradingRepository.DeleteDeal(deal);

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
