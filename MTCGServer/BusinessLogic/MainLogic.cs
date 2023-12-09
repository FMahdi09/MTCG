﻿using MTCGServer.BusinessLogic.Attributes;
using MTCGServer.BusinessLogic.Managers;
using MTCGServer.DataAccess.Repositories;
using MTCGServer.DataAccess.Repositorys;
using MTCGServer.Models.SerializationObjects;
using MTCGServer.Network.HTTP;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MTCGServer.BusinessLogic
{
    internal partial class MainLogic
    {
        // private variables

        // managers
        private readonly UserManager _userManager;
        private readonly TokenManager _tokenManager;
        private readonly CardManager _cardManager;

        // endpoint handlers
        private readonly Dictionary<Type, object> _handlers = new();

        // endpoint functions
        private readonly Dictionary<string[], MethodInfo> _getEndpoints = new();
        private readonly Dictionary<string[], MethodInfo> _postEndpoints = new();
        private readonly Dictionary<string[], MethodInfo> _putEndpoints = new();

        // constructor
        public MainLogic()
        {
            // create repositories
            string connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=mydb";
            UserRepository _userRepository = new(connectionString);
            TokenRepository _tokenRepository = new(connectionString);
            CardRepository _cardRepository = new(connectionString);

            // create managers
            _userManager = new UserManager(_userRepository);
            _tokenManager = new TokenManager(_tokenRepository);
            _cardManager = new CardManager(_cardRepository);
            

            // instantiate handlers
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute(typeof(EndPointHandlerAttribute)) != null &&
                    Activator.CreateInstance(type, _userManager, _tokenManager, _cardManager) is object handler)
                {
                    _handlers.Add(type, handler);

                    // map functions of each handler
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        if (method.GetCustomAttribute(typeof(EndPointAttribute)) is EndPointAttribute attr)
                        {
                            switch (attr.Method)
                            {
                                case HttpMethods.GET:
                                    _getEndpoints.Add(attr.Resource.Split('/', StringSplitOptions.RemoveEmptyEntries), method);
                                    break;
                                case HttpMethods.POST:
                                    _postEndpoints.Add(attr.Resource.Split('/', StringSplitOptions.RemoveEmptyEntries), method);
                                    break;
                                case HttpMethods.PUT:
                                    _putEndpoints.Add(attr.Resource.Split('/', StringSplitOptions.RemoveEmptyEntries), method);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        // public methods
        public HttpResponse HandleRequest(HttpRequest request)
        {
            if (request.Resource.Length < 1)
                return new HttpResponse("400 Bad Request", JsonSerializer.Serialize(new Error()
                {
                    ErrorMessage = "Invalid resource requested"
                }));

            // get endpoint function
            MethodInfo? endpoint = null;

            switch (request.Method)
            {
                case HttpMethods.GET:
                    endpoint = _getEndpoints.Where(x => x.Key.Length == request.Resource.Length && x.Key[0] == request.Resource[0])
                                            .Select(x => x.Value).FirstOrDefault();
                    break;
                case HttpMethods.POST:
                    endpoint = _postEndpoints.Where(x => x.Key.Length == request.Resource.Length && x.Key[0] == request.Resource[0])
                                             .Select(x => x.Value).FirstOrDefault();
                    break;
                case HttpMethods.PUT:
                    endpoint = _putEndpoints.Where(x => x.Key.Length == request.Resource.Length && x.Key[0] == request.Resource[0])
                                            .Select(x => x.Value).FirstOrDefault();
                    break;
                default:
                    return new HttpResponse("400 Bad Request", JsonSerializer.Serialize(new Error()
                    {
                        ErrorMessage = "Invalid HTTP method"
                    }));
            }

            // call endpoint function with corresponding handler
            if (endpoint != null && endpoint.DeclaringType != null)
            {
                object handler = _handlers[endpoint.DeclaringType];

                return (HttpResponse?)endpoint.Invoke(handler, new object[] { request }) ??
                    new HttpResponse("400 Bad Request", JsonSerializer.Serialize(new Error()
                    {
                        ErrorMessage = "Invalid resource requested"
                    }));
            }

            return new HttpResponse("400 Bad Request", JsonSerializer.Serialize(new Error()
                {
                    ErrorMessage = "Invalid resource requested"
                }));
        }
    }
}