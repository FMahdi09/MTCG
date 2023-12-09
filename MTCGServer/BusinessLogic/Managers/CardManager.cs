using MTCGServer.DataAccess.Repositories;
using MTCGServer.Models.DataModels;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.BusinessLogic.Managers
{
    public class CardManager
    {
        // private variables
        private readonly CardRepository _cardRepository;

        // constructor
        public CardManager(CardRepository cardRepository)
        {
            _cardRepository = cardRepository;
        }

        // public methods
        public List<Card> GenerateCards(int numberToGenerate)
        {
            return _cardRepository.GenerateCards(numberToGenerate);
        }

        public bool AssignCards(int userId, List<Card> toAssign) 
        { 
            try
            {
                _cardRepository.AssignCards(userId, toAssign);
            }
            catch(PostgresException)
            {
                return false;
            }
            return true;
        }

        public List<Card> GetCardsForUser(int userId) 
        {
            return _cardRepository.GetCards(userId);
        }
    }
}
