using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.Models.DataModels
{
    public class Card
    {
        public int CardId { get; set; } = -1;
        public string CardToken { get; set; } = "";
        public string Name { get; set; } = "";
        public int Damage { get; set; } = -1;
        public string Element { get; set; } = "";
        public string CardType { get; set; } = "";
    }
}
