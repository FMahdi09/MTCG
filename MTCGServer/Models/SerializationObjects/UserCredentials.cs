using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MTCGServer.Models.SerializationObjects
{
    public class UserCredentials
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
