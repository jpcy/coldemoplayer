using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CDP
{
    public interface IMediator
    {
        void Register<T>(Messages message, Action<T> callback, object registerer);
        void Unregister(Messages message, object registerer);
        void Notify<T>(Messages message, T parameter);
    }

    [Core.Singleton]
    public class Mediator : IMediator
    {
        public class MessageNotFoundException : Exception
        {
            public MessageNotFoundException()
                : base("Message does not exist.")
            {
            }
        }

        public class AlreadyRegisteredException : Exception
        {
            public AlreadyRegisteredException()
                : base("This registerer is already registered to receive notifications from this message.")
            {
            }
        }

        public class NotRegisteredException : Exception
        {
            public NotRegisteredException() : base("The specified registerer is not registered for this message.")
            {
            }
        }

        private class Registration
        {
            public object Callback { get; private set; }
            public object Registerer { get; private set; }

            public Registration(object callback, object registerer)
            {
                Callback = callback;
                Registerer = registerer;
            }
        }

        private Dictionary<Messages, List<Registration>> registrations = new Dictionary<Messages, List<Registration>>();

        public void Register<T>(Messages message, Action<T> callback, object registerer)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            if (registerer == null)
            {
                throw new ArgumentNullException("registerer");
            }

            if (!registrations.ContainsKey(message))
            {
                registrations.Add(message, new List<Registration>());
            }
            else
            {
                if (registrations[message].Exists(r => r.Registerer == registerer))
                {
                    throw new AlreadyRegisteredException();
                }
            }

            registrations[message].Add(new Registration(callback, registerer));
        }

        public void Unregister(Messages message, object registerer)
        {
            if (registerer == null)
            {
                throw new ArgumentNullException("registerer");
            }

            if (registrations.ContainsKey(message))
            {
                Registration reg = registrations[message].FirstOrDefault(r => r.Registerer == registerer);

                if (reg == null)
                {
                    throw new NotRegisteredException();
                }

                registrations[message].Remove(reg);
            }
            else
            {
                throw new NotRegisteredException();
            }
        }

        public void Notify<T>(Messages message, T parameter)
        {
            if (registrations.ContainsKey(message))
            {
                foreach (Registration r in registrations[message])
                {
                    MethodInfo methodInfo = r.Callback.GetType().GetMethod("Invoke");
                    methodInfo.Invoke(r.Callback, new object[] { parameter });
                }
            }
        }
    }
}
