using MTCGServer.DataAccess.Base;
using MTCGServer.Models.SerializationObjects;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.DataAccess.Repositories
{
    internal class ScoreRepository : BaseRepository
    {
        // constructor
        public ScoreRepository(IDbConnection connection)
            : base(connection) { }

        // READ

        public UserStats? GetStats(int userId)
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "SELECT username, score, wins, losses " +
                                   "FROM users " +
                                   "WHERE id = @id";

            // add parameters
            command.AddParameterWithValue("id", DbType.Int32, userId);

            // execute command
            using IDataReader reader = ExecuteQuery(command); 

            if(reader.Read()) 
            {
                return new UserStats()
                {
                    Name = (string)reader["username"],
                    Score = (int)reader["score"],
                    Wins = (int)reader["wins"],
                    Losses = (int)reader["losses"]
                };
            }
            return null;
        }

        public List<UserStats> GetScoreBoard(int amount)
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "SELECT username, score, wins, losses " +
                                  "FROM users " +
                                  "ORDER BY score DESC " +
                                  "LIMIT @amount";

            // add parameters
            command.AddParameterWithValue("amount", DbType.Int32, amount);

            // execute command
            using IDataReader reader = ExecuteQuery(command);

            List<UserStats> scoreboard = new();

            while(reader.Read()) 
            {
                UserStats stats = new()
                {
                    Name = (string)reader["username"],
                    Score = (int)reader["score"],
                    Wins = (int)reader["wins"],
                    Losses = (int)reader["losses"]
                };
                scoreboard.Add(stats);
            }

            return scoreboard;
        }
    }
}
