using MTCGServer.DataAccess.Repositorys;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.BusinessLogic.Managers
{
    public class TokenManager
    {
        // private variables
        private readonly TokenRepository _tokenRepository;

        // constructor
        public TokenManager(TokenRepository tokenRepository)
        {
            _tokenRepository = tokenRepository;
        }

        // public methods
        public string GenerateTokenForUser(int userId)
        {
            string token = Guid.NewGuid().ToString();
            _tokenRepository.Add(token, userId);
            return token;
        }

        public bool CheckToken(string token, string username)
        {
            throw new NotImplementedException();
        }
    }
}
