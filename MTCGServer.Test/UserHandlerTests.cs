using MTCGServer.BusinessLogic.EndPointHandlers;
using MTCGServer.BusinessLogic.Managers;
using MTCGServer.DataAccess.Repositories;
using MTCGServer.DataAccess.Repositorys;
using MTCGServer.Models.SerializationObjects;
using MTCGServer.Network.HTTP;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MTCGServer.Test
{
    internal class UserHandlerTests
    {
        private UserHandler _userHandler;

        [SetUp]
        public void Setup()
        {
            // create repositories
            string connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=testdb";
            UserRepository _userRepository = new(connectionString);
            TokenRepository _tokenRepository = new(connectionString);
            CardRepository _cardRepository = new(connectionString);

            // create managers
            UserManager _userManager = new(_userRepository);
            TokenManager _tokenManager = new(_tokenRepository);
            CardManager _cardManager = new(_cardRepository);

            // create userhandler
            _userHandler = new UserHandler(_userManager, _tokenManager, _cardManager);

            // clear db
            // create Connection
            using IDbConnection connection = new Npgsql.NpgsqlConnection(connectionString);
            connection.Open();

            // create Command
            using IDbCommand command = connection.CreateCommand();
            command.CommandText = "TRUNCATE users, tokens, createdcards";

            command.ExecuteNonQuery();
        }

        [Test]
        public void PostUsers()
        {
            // arrange            
            UserCredentials data = new()
            {
                Username = "Bob",
                Password = "password"
            };

            string[] resource = { "users" };

            HttpRequest request= new(HttpMethods.GET, resource, "", JsonSerializer.Serialize(data));

            // act
            HttpResponse responseSuccess = _userHandler.PostUser(request);
            HttpResponse responseFailed = _userHandler.PostUser(request);

            // assert
            Assert.That(responseSuccess.Status, Is.EqualTo("201 Created"));
            Assert.That(responseFailed.Status, Is.EqualTo("409 Conflict"));
        }
    }
}
