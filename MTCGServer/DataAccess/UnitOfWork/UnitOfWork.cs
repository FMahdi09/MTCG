using MTCGServer.DataAccess.Repositories;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCGServer.DataAccess.UnitOfWork
{
    internal class UnitOfWork : IDisposable
    {
        private readonly NpgsqlConnection _connection;
        private readonly NpgsqlTransaction? _transaction;

        private bool _disposed = false;
        private bool _committed = false;

        // repositories:
        private DeckRepository? _deckRepository;
        private UserRepository? _userRepository;
        private TokenRepository? _tokenRepository;
        private CardRepository? _cardRepository;
        private ScoreRepository? _scoreRepository;
        private TradingRepository? _tradeRepository;
        private BattleRepository? _battleRepository;

        public DeckRepository DeckRepository
        {
            get
            {
                _deckRepository ??= new(_connection);
                return _deckRepository;
            }
        }

        public UserRepository UserRepository
        {
            get
            {
                _userRepository ??= new(_connection);
                return _userRepository;
            }
        }

        public TokenRepository TokenRepository
        {
            get
            {
                _tokenRepository ??= new(_connection);
                return _tokenRepository;
            }
        }

        public CardRepository CardRepository
        {
            get
            {
                _cardRepository ??= new(_connection);
                return _cardRepository;
            }
        }

        public ScoreRepository ScoreRepository
        {
            get
            {
                _scoreRepository ??= new(_connection);
                return _scoreRepository;
            }
        }

        public TradingRepository TradingRepository
        {
            get
            {
                _tradeRepository ??= new(_connection);
                return _tradeRepository;
            }
        }

        public BattleRepository BattleRepository
        {
            get
            {
                _battleRepository ??= new(_connection);
                return _battleRepository;
            }
        }


        // constructor
        public UnitOfWork(string connectionString, bool withTransaction)
        {            
            _connection = new NpgsqlConnection(connectionString);
            _connection.Open();

            if (withTransaction)
                _transaction = _connection.BeginTransaction();
        }

        // Commit
        public void Commit()
        {
            _transaction?.Commit();
            _committed = true;
        }

        // Disposing
        public void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)  
        {
            if (!_disposed)
            {
                if(!_committed && _transaction != null)
                {
                    _transaction.Rollback();
                }                
                
                if (_connection != null)
                {
                    _connection.Close();
                    _connection.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
