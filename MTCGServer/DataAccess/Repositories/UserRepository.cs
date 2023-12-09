using MTCGServer.Models.SerializationObjects;
using MTCGServer.Models.UserModels;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.DataAccess.Repositorys
{
    public class UserRepository
    {
        // private variables
        private readonly string _connectionString;

        // constructor
        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // public methods

        // READ
        public User? Get(string username, string password)
        {
            // create Connection
            using IDbConnection connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // create Command
            using IDbCommand command = connection.CreateCommand();           
            command.CommandText = "SELECT * FROM users " +
                                  "WHERE username = @username " +
                                  "AND password = @password";

            // add Parameters
            command.AddParameterWithValue("username", DbType.String, username);
            command.AddParameterWithValue("password", DbType.String, password);

            // execute Command
            using IDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new User()
                {
                    Id = (int)reader["id"],
                    Username = (string)reader["username"],
                    Password = (string)reader["password"],
                    Bio = (string)reader["bio"],
                    Image = (string)reader["image"],
                    Currency = (int)reader["currency"]
                };
            }

            return null;
        }

        public User? Get(string token)
        {
            // create Connection
            using IDbConnection connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // create Command
            using IDbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM users " +
                                  "WHERE id = (SELECT userid FROM tokens WHERE token = @token)";

            // add parameters
            command.AddParameterWithValue("token", DbType.String, token);

            // execute Command
            using IDataReader reader = command.ExecuteReader();

            if(reader.Read())
            {
                return new User()
                {
                    Id = (int)reader["id"],
                    Username = (string)reader["username"],
                    Password = (string)reader["password"],
                    Bio = (string)reader["bio"],
                    Image = (string)reader["image"],
                    Currency = (int)reader["currency"]
                };
            }
            return null;
        }

        // CREATE
        public void Add(User toAdd)
        {
            // create Connection
            using IDbConnection connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // create Command
            using IDbCommand command = connection.CreateCommand();            
            command.CommandText = "INSERT INTO users (username, password, bio, image) " +
                                  "VALUES (@username, @password, @bio, @image)";

            // add Parameters
            command.AddParameterWithValue("username", DbType.String, toAdd.Username);
            command.AddParameterWithValue("password", DbType.String, toAdd.Password);
            command.AddParameterWithValue("bio", DbType.String, toAdd.Bio);
            command.AddParameterWithValue("image", DbType.String, toAdd.Image);

            // execute Command
            command.ExecuteNonQuery();
        }

        // UPDATE
        public void Update(User user)
        {
            // create Connection
            using IDbConnection connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // create Command
            using IDbCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE users " +
                                  "SET bio = @bio, image = @image, username = @username " +
                                  "WHERE id = @id";

            // add Parameters
            command.AddParameterWithValue("bio", DbType.String, user.Bio);
            command.AddParameterWithValue("image", DbType.String, user.Image);
            command.AddParameterWithValue("username", DbType.String, user.Username);
            command.AddParameterWithValue("id", DbType.Int32, user.Id);

            // execute Command
            command.ExecuteNonQuery();
        }

        public void PayCurrency(int userId, int amountToPay)
        {
            // create Connection
            using IDbConnection connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // create Command
            using IDbCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE users " +
                                  "SET currency = currency - @amountToPay " +
                                  "WHERE id = @id";

            // add parameters
            command.AddParameterWithValue("amountToPay", DbType.Int32, amountToPay);
            command.AddParameterWithValue("id", DbType.Int32, userId);

            // execute command
            command.ExecuteNonQuery();
        }

        // DELETE
    }
}
