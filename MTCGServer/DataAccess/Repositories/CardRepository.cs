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
    internal class CardRepository : BaseRepository
    {
        // constructor
        public CardRepository(IDbConnection connection)
            : base(connection) { }

        // CREATE

        public List<Card> GenerateCards(int amount)
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "SELECT c.id, c.name, c.damage, e.elementtype, ct.cardtype FROM cards c " +
                                  "JOIN elements e ON c.element_id = e.id " +
                                  "JOIN cardtypes ct ON c.cardtype_id = ct.id " +
                                  "ORDER BY random() limit @amount";

            // add parameters
            command.AddParameterWithValue("amount", DbType.Int32, amount);

            // execute command
            using IDataReader reader = ExecuteQuery(command);

            List<Card> toReturn = new();

            while (reader.Read())
            {
                // create card
                Card newCard = new()
                {
                    CardId = (int)reader["id"],
                    CardToken = Guid.NewGuid().ToString(),
                    Name = (string)reader["name"],
                    Damage = (int)reader["damage"],
                    Element = (string)reader["elementtype"],
                    CardType = (string)reader["cardtype"]
                };
                toReturn.Add(newCard);
            }
            return toReturn;
        }

        public void AssignCards(int userId, List<Card> cards)
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "INSERT INTO createdcards (card_id, user_id, card_token) " +
                                  "VALUES (@card_id, @user_id, @card_token)";

            foreach (Card card in cards) 
            {
                // add parameters
                command.AddParameterWithValue("user_id", DbType.Int32, userId);
                command.AddParameterWithValue("card_id", DbType.Int32, card.CardId);
                command.AddParameterWithValue("card_token", DbType.String, card.CardToken);

                // execute command
                ExecuteNonQuery(command);
                command.Parameters.Clear();
            }
        }        

        // READ

        public List<Card> GetCards(int userId)
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "SELECT cc.card_token, c.name, c.damage, c.id, e.elementtype, ct.cardtype " +
                                  "FROM createdcards cc " +
                                  "JOIN cards c ON cc.card_id = c.id " +
                                  "JOIN elements e ON c.element_id = e.id " +
                                  "JOIN cardtypes ct ON c.cardtype_id = ct.id " +
                                  "WHERE cc.user_id = @user_id";

            // add parameter
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

        public Card? GetCard(int userId, string cardToken)
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "SELECT cc.card_token, c.name, c.damage, c.id, e.elementtype, ct.cardtype " +
                                  "FROM createdcards cc " +
                                  "JOIN cards c ON cc.card_id = c.id " +
                                  "JOIN elements e ON c.element_id = e.id " +
                                  "JOIN cardtypes ct ON c.cardtype_id = ct.id " +
                                  "WHERE cc.card_token = @card_token " +
                                  "AND cc.user_id = @user_id";

            // add parameters
            command.AddParameterWithValue("card_token", DbType.String, cardToken);
            command.AddParameterWithValue("user_id", DbType.Int32, userId);

            // execute command
            using IDataReader reader = ExecuteQuery(command);

            if(reader.Read())
            {
                return new()
                {
                    CardId = (int)reader["id"],
                    CardToken = (string)reader["card_token"],
                    Name = (string)reader["name"],
                    Damage = (int)reader["damage"],
                    Element = (string)reader["elementtype"],
                    CardType = (string)reader["cardtype"]
                };
            }
            return null;
        }

        public bool CheckOwnership(int userId, List<string> cards_tokens) 
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "SELECT id FROM createdcards " +
                                  "WHERE user_id = @user_id " +
                                  "AND card_token = @card_token";

            foreach (string card_token in cards_tokens) 
            {
                // add parameters
                command.AddParameterWithValue("user_id", DbType.Int32, userId);
                command.AddParameterWithValue("card_token", DbType.String, card_token);

                // execute command
                using IDataReader reader = ExecuteQuery(command);

                if (!reader.Read())
                    return false;

                command.Parameters.Clear();
            }
            return true;
        }

        public bool CheckOwnership(int userId, Card card)
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "SELECT id FROM createdcards " +
                                  "WHERE user_id = @user_id " +
                                  "AND card_token = @card_token";

            // add parameters
            command.AddParameterWithValue("user_id", DbType.Int32, userId);
            command.AddParameterWithValue("card_token", DbType.String, card.CardToken);

            // execute command
            using IDataReader reader = ExecuteQuery(command);

            return reader.Read();
        }

        // UPDATE

        public void UnassignCard(int userId, string cardToken)
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "UPDATE createdcards " +
                                  "SET user_id = NULL " +
                                  "WHERE user_id = @user_id " +
                                  "AND card_token = @card_token";

            // add parameters
            command.AddParameterWithValue("user_id", DbType.Int32, userId);
            command.AddParameterWithValue("card_token", DbType.String, cardToken);

            // execute command
            ExecuteNonQuery(command);
        }

        public void ReassignCard(int userId, Card card)
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "UPDATE createdcards " +
                                  "SET user_id = @user_id " +
                                  "WHERE card_token = @card_token";


            // add parameters
            command.AddParameterWithValue("user_id", DbType.Int32, userId);
            command.AddParameterWithValue("card_token", DbType.String, card.CardToken);

            // execute command
            ExecuteNonQuery(command);
        }
    }
}
