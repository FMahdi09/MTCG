using MTCGServer.DataAccess.Base;
using MTCGServer.Models.DataModels;
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
    internal class TradingRepository : BaseRepository
    {
        // constructor
        public TradingRepository(IDbConnection connection)
            : base(connection) { }

        // CREATE

        public void CreateDeal(Tradingdeal deal)
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "INSERT INTO tradingdeals (trade_token, card_id, user_id, min_damage, cardtype_id)" +
                                  "VALUES (" +
                                      "@trade_token, " +
                                      "(SELECT id FROM createdcards WHERE card_token = @card_token), " +
                                      "@user_id, " +
                                      "@min_damage, " +
                                      "(SELECT id FROM cardtypes WHERE cardtype = @cardtype)" +
                                  ")";

            // add parameters
            command.AddParameterWithValue("trade_token", DbType.String, deal.TradeId);
            command.AddParameterWithValue("card_token", DbType.String, deal.Card.CardToken);
            command.AddParameterWithValue("user_id", DbType.Int32, deal.UserId);
            command.AddParameterWithValue("min_damage", DbType.Int32, deal.MinDamage);
            command.AddParameterWithValue("cardtype", DbType.String, deal.CardType);

            // execute command
            ExecuteNonQuery(command);
        }

        // READ

        public List<Tradingdeal> GetDeals(int userId = -1)
        {
            // create command
            using NpgsqlCommand command = new();

            if(userId != -1)
            {
                command.CommandText = "SELECT td.trade_token, td.user_id, td.min_damage, ct_deal.cardtype AS deal_cardtype, cc.card_token, c.name, c.damage, c.id, e.elementtype, ct_card.cardtype AS card_cardtype " +
                                      "FROM tradingdeals td " +
                                      "JOIN createdcards cc ON td.card_id = cc.id " +
                                      "JOIN cards c ON cc.card_id = c.id " +
                                      "JOIN elements e ON c.element_id = e.id " +
                                      "JOIN cardtypes ct_card ON c.cardtype_id = ct_card.id " +
                                      "JOIN cardtypes ct_deal ON td.cardtype_id = ct_deal.id " +
                                      "WHERE td.user_id = @user_id";

                command.AddParameterWithValue("user_id", DbType.Int32 , userId);
            }
            else
            {
                command.CommandText = "SELECT td.trade_token, td.user_id, td.min_damage, ct_deal.cardtype AS deal_cardtype, cc.card_token, c.name, c.damage, c.id, e.elementtype, ct_card.cardtype AS card_cardtype " +
                                      "FROM tradingdeals td " +
                                      "JOIN createdcards cc ON td.card_id = cc.id " +
                                      "JOIN cards c ON cc.card_id = c.id " +
                                      "JOIN elements e ON c.element_id = e.id " +
                                      "JOIN cardtypes ct_card ON c.cardtype_id = ct_card.id " +
                                      "JOIN cardtypes ct_deal ON td.cardtype_id = ct_deal.id";
            }

            // execute command
            using IDataReader reader = ExecuteQuery(command);

            List<Tradingdeal> deals = new();

            while(reader.Read()) 
            {
                Card card = new()
                {
                    CardId = (int)reader["id"],
                    CardToken = (string)reader["card_token"],
                    Name = (string)reader["name"],
                    Damage = (int)reader["damage"],
                    Element = (string)reader["elementtype"],
                    CardType = (string)reader["card_cardtype"]
                };

                Tradingdeal deal = new(tradeId: (string)reader["trade_token"],
                                       userId: (int)reader["user_id"],
                                       card: card,
                                       minDamage: (int)reader["min_damage"],
                                       cardType: (string)reader["deal_cardtype"]);

                deals.Add(deal);
            }
            return deals;
        }

        public Tradingdeal? GetDeal(string tradeToken)
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "SELECT td.trade_token, td.user_id, td.min_damage, ct_deal.cardtype AS deal_cardtype, cc.card_token, c.name, c.damage, c.id, e.elementtype, ct_card.cardtype AS card_cardtype " +
                                    "FROM tradingdeals td " +
                                    "JOIN createdcards cc ON td.card_id = cc.id " +
                                    "JOIN cards c ON cc.card_id = c.id " +
                                    "JOIN elements e ON c.element_id = e.id " +
                                    "JOIN cardtypes ct_card ON c.cardtype_id = ct_card.id " +
                                    "JOIN cardtypes ct_deal ON td.cardtype_id = ct_deal.id " +
                                    "WHERE td.trade_token = @trade_token";

            // add parameters
            command.AddParameterWithValue("trade_token", DbType.String, tradeToken);

            // execute command
            using IDataReader reader = ExecuteQuery(command);

            if(reader.Read())
            {
                Card card = new()
                {
                    CardId = (int)reader["id"],
                    CardToken = (string)reader["card_token"],
                    Name = (string)reader["name"],
                    Damage = (int)reader["damage"],
                    Element = (string)reader["elementtype"],
                    CardType = (string)reader["card_cardtype"]
                };

                Tradingdeal deal = new(tradeId: (string)reader["trade_token"],
                                       userId: (int)reader["user_id"],
                                       card: card,
                                       minDamage: (int)reader["min_damage"],
                                       cardType: (string)reader["deal_cardtype"]);

                return deal;
            }
            return null;
        }

        // DELETE

        public void DeleteDeal(Tradingdeal deal)
        {
            // create command
            using NpgsqlCommand command = new();
            command.CommandText = "DELETE FROM tradingdeals " +
                                  "WHERE trade_token = @trade_token";

            // add parameters
            command.AddParameterWithValue("trade_token", DbType.String, deal.TradeId);

            // execute command
            ExecuteNonQuery(command);
        }
    }
}
