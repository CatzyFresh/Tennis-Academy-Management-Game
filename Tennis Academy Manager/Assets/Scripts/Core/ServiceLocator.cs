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
            if (services.TryGetValue(typeof(T), out var svc))
                return svc as T;

            throw new KeyNotFoundException($"[ServiceLocator] Service not registered: {typeof(T).Name}");
        }

        public static bool TryGet<T>(out T service) where T : class, IGameService
        {
            if (services.TryGetValue(typeof(T), out var svc))
            {
                service = svc as T;
                return true;
            }
            service = null;
            return false;
        }
    }
}
