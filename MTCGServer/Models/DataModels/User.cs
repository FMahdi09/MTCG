using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.Models.UserModels
{
    public class User
    {
        public int Id { get; set; } = -1;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Bio { get; set; } = "";
        public string Image { get; set; } = "";
        public int Currency { get; set; } = 0;
    }
}
