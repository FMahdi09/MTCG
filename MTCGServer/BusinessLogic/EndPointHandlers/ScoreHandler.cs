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
    internal class ScoreHandler
    {
        // constants
        const int ScoreboardSize = 10;

        // private variables
        private readonly string _connectionString;

        // constructor
        public ScoreHandler(string connectionString)
        {
            _connectionString = connectionString;
        }

        [EndPoint(HttpMethods.GET, "/stats")]
        public HttpResponse GetStats(HttpRequest request) 
        { 
            try
            {
                // create UnitOfWork
                using UnitOfWork unit = new(_connectionString, withTransaction: false);

                // get user from token and check permissions
                if (unit.UserRepository.GetUser(request.Token) is not User user)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");

                // get stats from user
                if (unit.ScoreRepository.GetStats(user.Id) is not UserStats stats)
                    throw new HttpException("500 Internal Server Error", "Unable to retrieve Score for provided User");

                return new HttpResponse("200 OK", JsonSerializer.Serialize(stats));
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

        [EndPoint(HttpMethods.GET, "/scoreboard")]
        public HttpResponse GetScoreboard(HttpRequest request) 
        { 
            try
            {
                // create UnitOfWork
                using UnitOfWork unit = new(_connectionString, withTransaction: false);

                // get user from token and check permissions
                if (unit.UserRepository.GetUser(request.Token) is not User user)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");

                // get scoreboard
                List<UserStats> scoreboard = unit.ScoreRepository.GetScoreBoard(ScoreboardSize);

                return new HttpResponse("200 OK", JsonSerializer.Serialize(scoreboard));
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
