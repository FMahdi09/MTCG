using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.Models.SerializationObjects
{
    internal class UserStats
    {
        public string? Name { get; set; }
        public int? Score { get; set; }
        public int? Wins { get; set; }
        public int? Losses { get; set; }
    }
}
