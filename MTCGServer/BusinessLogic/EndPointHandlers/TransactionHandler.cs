﻿using MTCGServer.BusinessLogic.Attributes;
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

namespace MTCGServer.Businesslogic.EndPointHandlers
{
    [EndPointHandler]
    internal class TransactionHandler
    {
        // public variables
        public const int PackagePrice = 5;
        public const int CardsPerPackage = 5;

        // private variables
        private readonly string _connectionString;

        // constructor
        public TransactionHandler(string connectionstring) 
        {
            _connectionString = connectionstring;
        }
        

        [EndPoint(HttpMethods.POST, "/transactions/packages")]
        public HttpResponse PostPackage(HttpRequest request) 
        {           
            try
            {
                // create UnitOfWork
                using UnitOfWork unit = new(_connectionString, withTransaction: true);

                // get user from token and check permissions
                if (unit.UserRepository.GetUser(request.Token) is not User user)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");

                // make user pay
                unit.UserRepository.PayCurrency(user.Id, PackagePrice);                    

                // generate cards for user
                List<Card> generatedCards = unit.CardRepository.GenerateCards(CardsPerPackage);

                // assign cards to user
                unit.CardRepository.AssignCards(user.Id, generatedCards);

                // commit work
                unit.Commit();

                return new HttpResponse("200 OK", JsonSerializer.Serialize(generatedCards));
            }
            catch (HttpException ex)
            {
                return new HttpResponse(ex.Header, JsonSerializer.Serialize(ex.Error));
            }
            catch (PostgresException ex)
            {
                return ex.SqlState switch
                {
                    // 23514: check constraint violation
                    "23514" => new HttpResponse("409 Confilct", JsonSerializer.Serialize(new Error("Not enough money for buying a card package"))),

                    // default
                    _ => new HttpResponse("500 Internal Server Error")
                };
            }
            catch (NpgsqlException)
            {
                return new HttpResponse("500 Internal Server Error", JsonSerializer.Serialize(new Error("Unable to connect to database")));
            }
        }        
    }
}
