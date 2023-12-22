using MTCGServer.DataAccess.Base;
using MTCGServer.Models.DataModels;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.DataAccess.Repositories
{
    internal class UserRepository : BaseRepository
    {
        // constructor
        public UserRepository(IDbConnection connection)
            : base(connection) { }

        // CREATE

        public void CreateUser(User user)
        {
            // create Command
            using NpgsqlCommand command = new ();
            command.CommandText = "INSERT INTO users (username, password, bio, image) " +
                                  "VALUES (@username, @password, @bio, @image)";

            // add Parameters
            command.AddParameterWithValue("username", DbType.String, user.Username);
            command.AddParameterWithValue("password", DbType.String, user.Password);
            command.AddParameterWithValue("bio", DbType.String, user.Bio);
            command.AddParameterWithValue("image", DbType.String, user.Image);

            // execute query
            ExecuteNonQuery(command);
        }

        // READ

        public User? GetUser(string username, string password)
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "SELECT * FROM users " +
                                  "WHERE username = @username " +
                                  "AND password = @password";

            // add parameters
            command.AddParameterWithValue("username", DbType.String, username);
            command.AddParameterWithValue("password", DbType.String, password);

            // execute query
            using IDataReader reader = ExecuteQuery(command);

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

        public User? GetUser(string token)
        {
            // create Command
            using NpgsqlCommand command = new();
            command.CommandText = "SELECT * FROM users " +
                                  "WHERE id = (SELECT userid FROM tokens WHERE token = @token)";

            // add parameters
            command.AddParameterWithValue("token", DbType.String, token);

            // execute command
            using IDataReader reader = ExecuteQuery(command);

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


        // UPDATE

        public void Update(User user)
        {
            // create Command
            using NpgsqlCommand command = new();
            command.CommandText = "UPDATE users " +
                                  "SET bio = @bio, image = @image, username = @username " +
                                  "WHERE id = @id";

            // add parameters
            command.AddParameterWithValue("bio", DbType.String, user.Bio);
            command.AddParameterWithValue("image", DbType.String, user.Image);
            command.AddParameterWithValue("username", DbType.String, user.Username);
            command.AddParameterWithValue("id", DbType.Int32, user.Id);

            // execute query
            ExecuteNonQuery (command);
        }

        public void PayCurrency(int userId, int amountToPay)
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "UPDATE users " +
                                  "SET currency = currency - @amountToPay " +
                                  "WHERE id = @id";

            // add parameters
            command.AddParameterWithValue("amountToPay", DbType.Int32, amountToPay);
            command.AddParameterWithValue("id", DbType.Int32, userId);

            // execute query
            ExecuteNonQuery(command);
        }
    }
}
