using System;
using System.Collections.Generic;

namespace TennisAcademyManager.Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IGameService> services = new();

        public static void Register<T>(T service) where T : class, IGameService
        {
            services[typeof(T)] = service;
        }

        public static T Get<T>() where T : class, IGameService
        {
            return services[typeof(T)] as T;
        }
    }
}
