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
    internal class BattleHandler
    {
        // private variables
        private readonly string _connectionString;

        // constructor
        public BattleHandler(string connectionString)
        {
            _connectionString = connectionString;
        }

        [EndPoint(HttpMethods.POST, "/battles")]
        public HttpResponse PostBattle(HttpRequest request)
        {
            try
            {
                // create UnitOfWork
                using UnitOfWork unit = new(_connectionString, withTransaction: true);

                // get user from token and check permissions
                if (unit.UserRepository.GetUser(request.Token) is not User user)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");

                // enter user in queue
                unit.BattleRepository.EnterQueue(user);

                // wait for battle result

                // commit work
                unit.Commit();

                // return battle result

                return new HttpResponse("200 OK");
            }
            catch (HttpException ex)
            {
                return new HttpResponse(ex.Header, JsonSerializer.Serialize(ex.Error));
            }
            catch (PostgresException ex)
            {
                return ex.SqlState switch
                {
                    // 23505: unique constraint violation
                    "23505" => new HttpResponse("409 Confilct", JsonSerializer.Serialize(new Error("Already in queue"))),

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
