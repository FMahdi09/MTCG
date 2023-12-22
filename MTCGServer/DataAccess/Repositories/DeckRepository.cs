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
    internal class DeckRepository : BaseRepository
    {
        // constructor
        public DeckRepository(IDbConnection connection)
            : base(connection) { }

        // READ

        public List<Card> GetDeck(int userId)
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "SELECT cc.card_token, c.name, c.damage, c.id, e.elementtype, ct.cardtype " +
                                  "FROM createdcards cc " +
                                  "JOIN cards c ON cc.card_id = c.id " +
                                  "JOIN elements e ON c.element_id = e.id " +
                                  "JOIN cardtypes ct ON c.cardtype_id = ct.id " +
                                  "WHERE cc.user_id = @user_id " +
                                  "AND cc.is_in_deck = TRUE";

            // add parameters
            command.AddParameterWithValue("user_id", DbType.Int32, userId);

            // execute query
            using IDataReader reader = ExecuteQuery(command);

            List<Card> toReturn = new();

            while (reader.Read())
            {
                Card card = new()
                {
                    CardId = (int)reader["id"],
                    CardToken = (string)reader["card_token"],
                    Name = (string)reader["name"],
                    Damage = (int)reader["damage"],
                    Element = (string)reader["elementtype"],
                    CardType = (string)reader["cardtype"]
                };
                toReturn.Add(card);
            }

            return toReturn;
        }

        public bool IsInDeck(int userId, Card card) 
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "SELECT id " +
                                  "FROM createdcards " +
                                  "WHERE user_id = @user_id " +
                                  "AND card_token = @card_token " +
                                  "AND is_in_deck = TRUE";

            // add parameters
            command.AddParameterWithValue("user_id", DbType.Int32, userId);
            command.AddParameterWithValue("card_token", DbType.String, card.CardToken);

            // execute command
            using IDataReader reader = ExecuteQuery(command);

            return reader.Read();
        }

        // UPDATE

        public void ClearDeck(int userId) 
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "UPDATE createdcards " +
                                  "SET is_in_deck = FALSE " +
                                  "WHERE user_id = @user_id";

            // add parameters
            command.AddParameterWithValue("user_id", DbType.Int32, userId);

            // execute command
            ExecuteNonQuery(command);
        }

        public void UpdateDeck(int userId, List<string> card_tokens) 
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "UPDATE createdcards " +
                                  "SET is_in_deck = TRUE " +
                                  "WHERE user_id = @user_id " +
                                  "AND card_token = @card_token";

            foreach (string card_token in card_tokens)
            {
                // add parameters
                command.AddParameterWithValue("user_id", DbType.Int32, userId);
                command.AddParameterWithValue("card_token", DbType.String, card_token);

                // execute command
                ExecuteNonQuery(command);
                command.Parameters.Clear();
            }
        }
    }
}
