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
    internal class SessionHandlerTests
    {
        private SessionHandler _sessionHandler;

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
            _sessionHandler = new SessionHandler(_userManager, _tokenManager, _cardManager);

            // clear db
            // create Connection
            using IDbConnection connection = new Npgsql.NpgsqlConnection(connectionString);
            connection.Open();

            // create Command
            using IDbCommand command = connection.CreateCommand();
            command.CommandText = "TRUNCATE users, tokens, createdcards";

            command.ExecuteNonQuery();

            // populate db
            _userManager.AddUser("Bob", "password");
        }

        [Test]
        public void PostSession()
        {
            // arrange
            UserCredentials dataSuccess = new()
            {
                Username = "Bob",
                Password = "password"
            };

            string[] resource = { "sessions" };

            HttpRequest requestSuccess = new(HttpMethods.POST, resource, "", JsonSerializer.Serialize(dataSuccess));

            UserCredentials dataFailed = new()
            {
                Username = "Bob",
                Password = "wrong password"
            };

            HttpRequest requestFailed = new(HttpMethods.POST, resource, "", JsonSerializer.Serialize(dataFailed));

            // act
            HttpResponse responseSuccess = _sessionHandler.PostSession(requestSuccess);
            HttpResponse responseFailed = _sessionHandler.PostSession(requestFailed);

            // assert
            Assert.That(responseSuccess.Status, Is.EqualTo("200 OK"));
            Assert.That(responseFailed.Status, Is.EqualTo("401 Unauthorized"));
        }
    }
}
