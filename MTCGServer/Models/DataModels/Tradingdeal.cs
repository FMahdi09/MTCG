using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.Models.DataModels
{
    internal class Tradingdeal
    {
        public string TradeId { get; set; }
        public int UserId { get; set; }
        public Card Card { get; set; }
        public int MinDamage { get; set; }
        public string CardType { get; set; }

        public Tradingdeal(string tradeId, int userId, Card card, int minDamage, string cardType) 
        { 
            TradeId = tradeId;
            UserId = userId;
            Card = card;
            MinDamage = minDamage;
            CardType = cardType;
        }
    }
}
