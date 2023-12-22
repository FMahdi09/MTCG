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
    internal class BattleRepository : BaseRepository
    {
        // constructor
        public BattleRepository(IDbConnection connection)
            : base(connection) { }

        // CREATE

        public void EnterQueue(User user)
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "INSERT INTO battlequeue (user_id) " +
                                  "VALUES (@user_id)";

            // add parameters
            command.AddParameterWithValue("user_id", DbType.Int32, user.Id);

            // execute command
            ExecuteNonQuery(command);
        }

        // READ
    }
}
