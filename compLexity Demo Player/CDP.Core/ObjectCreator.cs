using System;
using System.Linq;
using System.Collections.Generic;

namespace CDP.Core
{
    public interface IObjectProvider
    {
        T Get<T>(params object[] args);
    }

    public static class ObjectCreator
    {
        private class Mapping
        {
            public Type Type { get; private set; }
            public IObjectProvider Provider { get; private set; }

            public Mapping(Type type)
            {
                Type = type;
            }

            public Mapping(IObjectProvider provider)
            {
                Provider = provider;
            }
        }

        private static Dictionary<Type, Mapping> mappings = new Dictionary<Type, Mapping>();

        public static void Map<T1,T2>()
        {
            mappings.Add(typeof(T1), new Mapping(typeof(T2)));
        }

        public static void MapToProvider<T>(IObjectProvider provider)
        {
            mappings.Add(typeof(T), new Mapping(provider));
        }

        public static T Get<T>(params object[] args)
        {
            Mapping mapping = mappings[typeof(T)];

            if (mapping.Provider != null)
            {
                return mapping.Provider.Get<T>(args);
            }

            return (T)Activator.CreateInstance(mapping.Type, args);
        }

        public static void Reset()
        {
            mappings.Clear();
        }
    }
}