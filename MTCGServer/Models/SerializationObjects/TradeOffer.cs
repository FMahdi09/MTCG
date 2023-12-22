using MTCGServer.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.Models.SerializationObjects
{
    internal class TradeOffer
    {        
        public string? Cardtoken { get; set; }
        public int? MinDamage { get; set; }
        public string? CardType { get; set; }
    }
}
