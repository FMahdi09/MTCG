using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.Models.SerializationObjects
{
    internal class Error
    {
        public string ErrorMessage { get; set; }

        public Error(string message)
        {
            ErrorMessage = message;
        }
    }
}
