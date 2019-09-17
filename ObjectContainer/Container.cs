using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectContainer
{
    public class Container
    {
        private readonly List<Registration> _registrations = new List<Registration>();

        public object ResolveInstance(Type service, string key = null)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            var registration = _registrations.Where(x => x.Service == service && x.Key == key).FirstOrDefault();

            if (registration == null)
            {
                return null;
            }

            if (registration.Instance == null)
            {
                object instance = registration.Handler?.Invoke(this) ?? CreateInstanceOf(registration.Implementation);
                if (registration.IsSingleton)
                {
                    registration = registration.With(instance);
                }

                return instance;
            }

            return registration.Instance;
        }

        public void RegisterInstance(Type service, object instance, string key = null)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (_registrations.Exists(x => x.Service == service && x.Key == key))
            {
                throw new InvalidOperationException($"A registration for type {service.FullName} already exists!");
            }

            _registrations.Add(new Registration(service, instance.GetType(), false, instance, key, null));
        }

        public void RegisterPerRequest(Type service, Type implementation, string key = null)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (_registrations.Exists(x => x.Service == service && x.Key == key))
            {
                throw new InvalidOperationException($"A registration for type {service.FullName} already exists!");
            }

            _registrations.Add(new Registration(service, implementation, false, null, key, null));
        }

        public void RegisterSingleton(Type service, Type implementation, string key = null)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (_registrations.Exists(x => x.Service == service && x.Key == key))
            {
                throw new InvalidOperationException($"A registration for type {service.FullName} already exists!");
            }

            _registrations.Add(new Registration(service, implementation, true, null, key, null));
        }

        public void RegisterHandler(Type service, Func<Container, object> handler, string key = null)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (_registrations.Exists(x => x.Service == service && x.Key == key))
            {
                throw new InvalidOperationException($"A registration for type {service.FullName} already exists!");
            }

            _registrations.Add(new Registration(service, null, true, null, key, handler));
        }

        private object CreateInstanceOf(Type type)
        {
            foreach (var ctor in type.GetConstructors())
            {
                ParameterInfo[] parameters = ctor.GetParameters();
                object[] args = parameters.Select(x => ResolveInstance(x.ParameterType)).ToArray();

                try
                {
                    return Activator.CreateInstance(type, args);
                }
                catch (Exception)
                {
                    // Continue trying to create an instance
                }
            }

            return null;
        }

        private class Registration
        {
            public Registration(
                Type service,
                Type implementation,
                bool isSingleton,
                object instance,
                string key,
                Func<Container, object> handler)
            {
                if (implementation == null && handler == null)
                {
                    throw new ArgumentException(
                        $"{nameof(implementation)} Type or a {nameof(handler)} Func missing!");
                }

                Service = service ?? throw new ArgumentNullException(nameof(service));
                Implementation = implementation;
                IsSingleton = isSingleton;
                Instance = instance;
                Key = key;
                Handler = handler;
            }

            public Type Service { get; }
            public Type Implementation { get; }
            public bool IsSingleton { get; }
            public object Instance { get; }
            public string Key { get; }
            public Func<Container, object> Handler { get; }

            public Registration With(object instance = null) =>
                new Registration(Service, Implementation, IsSingleton, instance ?? Instance, Key, Handler);
        }
    }
}
