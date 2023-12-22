using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.DataAccess.Base
{
    internal abstract class BaseRepository
    {
        protected IDbConnection _connection;

        // constructor
        public BaseRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public IDataReader ExecuteQuery(IDbCommand command)
        {
            command.Connection = _connection;
            command.Prepare();

            return command.ExecuteReader();
        }

        public void ExecuteNonQuery(IDbCommand command) 
        {
            command.Connection = _connection;
            command.Prepare();

            command.ExecuteNonQuery();
        }
    }
}
