using System;
using System.Linq;
using System.Collections.Generic;

namespace CDP.Core
{
    public class Singleton : Attribute
    {
    }

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
        private static Dictionary<Type, object> singletons = new Dictionary<Type, object>();

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

            bool isSingleton = false;

            foreach (var attribute in Attribute.GetCustomAttributes(mapping.Type))
            {
                if (attribute is Singleton)
                {
                    isSingleton = true;
                    break;
                }
            }

            if (isSingleton)
            {
                if (singletons.ContainsKey(mapping.Type))
                {
                    return (T)singletons[mapping.Type];
                }
            }

            object result = Activator.CreateInstance(mapping.Type, args);

            if (isSingleton)
            {
                singletons.Add(mapping.Type, result);
            }

            return (T)result;
        }

        public static void Reset()
        {
            mappings.Clear();
            singletons.Clear();
        }
    }
}