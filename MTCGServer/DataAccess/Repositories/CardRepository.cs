using MTCGServer.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.DataAccess.Repositories
{
/*
create new card statement template :

INSERT INTO cards (name, damage, element_id, cardtype_id)
VALUES (
    'Ray of Light',
    60,
    (SELECT id FROM elements WHERE elementtype = 'light'),
    (SELECT id FROM cardtypes WHERE cardtype = 'spell')
);

*/
    public class CardRepository
    {
        // private variables
        private readonly string _connectionstring;

        // constructor
        public CardRepository(string connectionstring)
        {
            _connectionstring = connectionstring;
        }

        // public methods
        public List<Card> GetCards(int userId) 
        {
            // create Connection
            using IDbConnection connection = new Npgsql.NpgsqlConnection(_connectionstring);
            connection.Open();

            // create Command
            using IDbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT cc.card_token, c.name, c.damage, c.id, e.elementtype, ct.cardtype " +
                                  "FROM createdcards cc " +
                                  "JOIN cards c ON cc.card_id = c.id " +
                                  "JOIN elements e ON c.element_id = e.id " +
                                  "JOIN cardtypes ct ON c.cardtype_id = ct.id " +
                                  "WHERE cc.user_id = @user_id";

            // add parameters
            command.AddParameterWithValue("user_id", DbType.Int32, userId);

            // execute command
            using IDataReader reader = command.ExecuteReader();

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
        public List<Card> GenerateCards(int amount)
        {
            // create Connection
            using IDbConnection connection = new Npgsql.NpgsqlConnection(_connectionstring);
            connection.Open();

            // create Command
            using IDbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT c.id, c.name, c.damage, e.elementtype, ct.cardtype FROM cards c " +
                                  "JOIN elements e ON c.element_id = e.id " +
                                  "JOIN cardtypes ct ON c.cardtype_id = ct.id " +
                                  "ORDER BY random() limit @amount";

            // add parameters
            command.AddParameterWithValue("amount", DbType.Int32, amount);

            // execute Command
            using IDataReader reader = command.ExecuteReader();

            List<Card> toReturn = new();

            while(reader.Read()) 
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

        public void AssignCards(int userId, List<Card> toAssign) 
        {
            // create Connection
            using IDbConnection connection = new Npgsql.NpgsqlConnection(_connectionstring);
            connection.Open();

            // create Command
            using IDbCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO createdcards (card_id, user_id, card_token) " +
                                  "VALUES (@card_id, @user_id, @card_token)";

            // add parameters for each card
            command.AddParameterWithValue("user_id", DbType.Int32, userId);

            foreach(Card card in toAssign)
            {
                command.AddParameterWithValue("user_id", DbType.Int32, userId);
                command.AddParameterWithValue("card_id", DbType.Int32, card.CardId);
                command.AddParameterWithValue("card_token", DbType.String, card.CardToken);

                command.ExecuteNonQuery();
                command.Parameters.Clear();
            }
        }
    }
}
