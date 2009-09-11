using System;
using System.Linq;
using System.Collections.Generic;

namespace CDP.Core
{
    public class Singleton : Attribute
    {
    }

    public interface IObjectProvider<T> where T:class
    {
        T Get(params object[] args);
    }

    /// <summary>
    /// Handles mapping and resolving interfaces to their implementation classes.
    /// </summary>
    public static class ObjectCreator
    {
        private class Mapping
        {
            public Type Type { get; private set; }
            public object Provider { get; private set; }

            public Mapping(Type type)
            {
                Type = type;
            }

            public Mapping(object provider)
            {
                Provider = provider;
            }
        }

        private static Dictionary<Type, Mapping> mappings = new Dictionary<Type, Mapping>();
        private static Dictionary<Type, object> singletons = new Dictionary<Type, object>();

        public static void Map<T1,T2>() where T1:class where T2:class
        {
            mappings.Add(typeof(T1), new Mapping(typeof(T2)));
        }

        public static void MapToProvider<T>(IObjectProvider<T> provider) where T:class
        {
            mappings.Add(typeof(T), new Mapping(provider));
        }

        public static T Get<T>(params object[] args) where T:class
        {
            if (!mappings.ContainsKey(typeof(T)))
            {
                throw new ArgumentException(string.Format("No mapping for type '{0}' found.", typeof(T).ToString()));
            }

            Mapping mapping = mappings[typeof(T)];

            if (mapping.Provider != null)
            {
                IObjectProvider<T> provider = (IObjectProvider<T>)mapping.Provider;
                return provider.Get(args);
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