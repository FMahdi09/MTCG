using MTCGServer.Models.SerializationObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.BusinessLogic.Exceptions
{
    internal class HttpException : Exception
    {
        public string Header { get; } = "";
        public Error Error { get; } = new();

        public HttpException() { }
        public HttpException(string header, string errorMessage) 
            : base(errorMessage) 
        {
            Header = header;
            Error = new()
            {
                ErrorMessage = errorMessage
            };
        }
    }
}
