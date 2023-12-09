using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.Models.SerializationObjects
{
    public class UserData
    {
        public string? Username { get; set; }
        public string? Bio { get; set; }
        public string? Image { get; set; }
    }
}
