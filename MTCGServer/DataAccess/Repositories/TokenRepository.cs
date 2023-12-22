using MTCGServer.DataAccess.Base;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.DataAccess.Repositories
{
    internal class TokenRepository : BaseRepository
    {
        // constructor
        public TokenRepository(IDbConnection connection)
            : base(connection) { }

        // CREATE
        
        public void AddToken(int userId, string token)
        {
            // create command
            using IDbCommand command = new NpgsqlCommand();
            command.CommandText = "INSERT INTO tokens (token, userid) " +
                                  "VALUES (@token, @userid)";

            // add parameters
            command.AddParameterWithValue("token", DbType.String, token);
            command.AddParameterWithValue("userid", DbType.Int32, userId);

            // execute command
            ExecuteNonQuery(command);
        }
    }
}
