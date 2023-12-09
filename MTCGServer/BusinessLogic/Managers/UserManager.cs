using MTCGServer.DataAccess.Repositorys;
using MTCGServer.Models.SerializationObjects;
using MTCGServer.Models.UserModels;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.BusinessLogic.Managers
{
    public class UserManager
    {
        // private variables
        private readonly UserRepository _userRepository;

        //constructor
        public UserManager(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // public methods
        public bool AddUser(string username, string password)
        {
            User toAdd = new()
            {
                Username = username,
                Password = password
            };
            try
            {
                _userRepository.Add(toAdd);
            }
            catch (PostgresException)
            {
                return false;
            }

            return true;
        }

        public bool UpdateUser(User user)
        {
            try
            {
                _userRepository.Update(user);
            }
            catch (PostgresException)
            {
                return false;
            }

            return true;
        }
        public User? GetUser(string username, string password)
        {
            return _userRepository.Get(username, password);
        }
        public User? GetUser(string token)
        {
            return _userRepository.Get(token);
        }
        public bool PayCurrency(int userId, int amountToPay)
        {
            try
            {
                _userRepository.PayCurrency(userId, amountToPay);
            }
            catch(PostgresException) 
            {
                return false;
            }
            return true;
        }
    }
}
