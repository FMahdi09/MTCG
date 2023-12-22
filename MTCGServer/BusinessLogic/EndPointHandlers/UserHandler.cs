using MTCGServer.Network.HTTP;
using MTCGServer.BusinessLogic.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MTCGServer.Models.SerializationObjects;
using MTCGServer.BusinessLogic.Exceptions;
using MTCGServer.DataAccess.UnitOfWork;
using Npgsql;
using MTCGServer.Models.DataModels;

namespace MTCGServer.BusinessLogic.EndPointHandlers
{
    [EndPointHandler]
    public class UserHandler
    {
        // private variables
        private readonly string _connectionString;

        // constructor
        public UserHandler(string connectionstring)
        {
            _connectionString = connectionstring;
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
                if (userCredentials == null          ||
                    userCredentials.Username == null ||
                    userCredentials.Password == null)
                    throw new HttpException("400 Bad Request", "Invalid Body");

                // create UnitOfWork
                using UnitOfWork unit = new(_connectionString, withTransaction: false);

                // create new user
                User user = new()
                {
                    Username = userCredentials.Username,
                    Password = userCredentials.Password
                };

                unit.UserRepository.CreateUser(user);

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
                    // 23505: unique constraint violation
                    "23505" => new HttpResponse("409 Confilct", JsonSerializer.Serialize(new Error("Username already exists"))),

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

        [EndPoint(HttpMethods.GET, "/users/{username}")]
        public HttpResponse GetUser(HttpRequest request)
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
            catch (PostgresException)
            {
                return new HttpResponse("500 Internal Server Error");
            }
            catch (NpgsqlException)
            {
                return new HttpResponse("500 Internal Server Error", JsonSerializer.Serialize(new Error("Unable to connect to database")));
            }
        }

        [EndPoint(HttpMethods.PUT, "/users/{username}")]
        public HttpResponse PutUser(HttpRequest request)
        {            
            try
            {
                // get username from path
                string username = request.Resource[1];

                // deserialize body
                UserData? userData = JsonSerializer.Deserialize<UserData>(request.Body);

                // check if body is valid
                if (userData == null          ||
                    userData.Username == null ||
                    userData.Image == null    ||
                    userData.Bio == null)
                    throw new HttpException("400 Bad Request", "Invalid Body");

                // create UnitOfWork
                using UnitOfWork unit = new(_connectionString, withTransaction: false);

                // get user from token and check permissions
                if (unit.UserRepository.GetUser(request.Token) is not User user ||
                   user.Username != username)
                    throw new HttpException("401 Unauthorized", "Access token is missing or invalid");

                // update user
                user.Username = userData.Username;
                user.Image = userData.Image;
                user.Bio = userData.Bio;

                // update user in db
                unit.UserRepository.Update(user);

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
                    "23505" => new HttpResponse("409 Confilct", JsonSerializer.Serialize(new Error("Username already exists"))),

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
        
    }
}
