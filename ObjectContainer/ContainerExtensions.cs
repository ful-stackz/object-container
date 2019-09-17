using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectContainer
{
    public static class ContainerExtensions
    {
        public static TService Resolve<TService>(this Container container, string key = null) =>
            (TService)container.ResolveInstance(typeof(TService), key);

        public static void Instance<TService>(this Container container, object instance, string key = null) =>
            container.RegisterInstance(typeof(TService), instance, key);

        public static void Singleton<TImplementation>(this Container container, string key = null) =>
            container.RegisterSingleton(typeof(TImplementation), typeof(TImplementation), key);

        public static void Singleton<TService, TImplementation>(this Container container, string key = null)
            where TImplementation : TService =>
            container.RegisterSingleton(typeof(TService), typeof(TImplementation), key);

        public static void PerRequest<TImplementation>(this Container container, string key = null) =>
            container.RegisterPerRequest(typeof(TImplementation), typeof(TImplementation), key);

        public static void PerRequest<TService, TImplementation>(this Container container, string key = null)
            where TImplementation : TService =>
            container.RegisterPerRequest(typeof(TService), typeof(TImplementation), key);

        public static void Handler<TService>(this Container container, Func<Container, object> handler, string key = null) =>
            container.RegisterHandler(typeof(TService), handler, key);
    }
}
