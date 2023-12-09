using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.DataAccess.Repositorys
{
    public class TokenRepository
    {
        // private variables
        private readonly string _connectionString;

        // constructor
        public TokenRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // public methods

        // READ

        // CREATE
        public void Add(string token, int userId)
        {
            // create Connection
            using IDbConnection connection = new Npgsql.NpgsqlConnection(_connectionString);
            connection.Open();

            // create Command
            using IDbCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO tokens (token, userid) " +
                                  "VALUES (@token, @userid)";

            // add Parameters
            command.AddParameterWithValue("@token", DbType.String, token);
            command.AddParameterWithValue("@userid", DbType.Int64, userId);

            // execute Command
            command.ExecuteNonQuery();
        }

        // UPDATE

        // DELETE
    }
}
