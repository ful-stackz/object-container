using NUnit.Framework;
using System;

namespace ObjectContainer.Tests
{
    public class ContainerTests
    {
        private Container _container;

        [SetUp]
        public void Setup()
        {
            _container = new Container();
        }

        #region ResolveInstance

        [Test]
        public void ResolveInstance_ReturnsTheSameInstance_WhenResolvingSingletonWithPreRegisteredDependencies()
        {
            _container.RegisterInstance(typeof(string), "admin");
            _container.RegisterInstance(typeof(Credentials), new Credentials("admin", "admin@example.com"));
            _container.RegisterSingleton(typeof(IUser), typeof(User));

            var instanceA = (IUser)_container.ResolveInstance(typeof(IUser));
            var instanceB = (IUser)_container.ResolveInstance(typeof(IUser));

            Assert.AreSame(instanceA, instanceB);
        }
        
        [Test]
        public void ResolveInstance_ReturnsTheSameInstance_WhenResolvingSingletonWithSameKeyFromMultipleRegistrations()
        {
            // Register dependencies for User
            _container.RegisterInstance(typeof(string), "admin");
            _container.RegisterInstance(typeof(Credentials), new Credentials("admin", "admin@example.com"));
            // Register multiple singletons for IUser with different keys
            const string AdminKey = "admin";
            const string ModKey = "moderator";
            _container.RegisterSingleton(typeof(IUser), typeof(User), AdminKey);
            _container.RegisterSingleton(typeof(IUser), typeof(User), ModKey);

            var instanceA = (IUser)_container.ResolveInstance(typeof(IUser), AdminKey);
            var instanceB = (IUser)_container.ResolveInstance(typeof(IUser), AdminKey);

            Assert.AreSame(instanceA, instanceB);
        }

        [Test]
        public void ResolveInstance_ReturnsDifferentInstances_WhenResolvingSingletonWithDifferentKeyFromMultipleRegistrations()
        {
            // Register dependencies for User
            _container.RegisterInstance(typeof(string), "admin");
            _container.RegisterInstance(typeof(Credentials), new Credentials("admin", "admin@example.com"));
            // Register multiple singletons for IUser with different keys
            const string AdminKey = "admin";
            const string ModKey = "moderator";
            _container.RegisterSingleton(typeof(IUser), typeof(User), AdminKey);
            _container.RegisterSingleton(typeof(IUser), typeof(User), ModKey);

            var instanceA = (IUser)_container.ResolveInstance(typeof(IUser), AdminKey);
            var instanceB = (IUser)_container.ResolveInstance(typeof(IUser), ModKey);

            Assert.AreNotSame(instanceA, instanceB);
        }

        #endregion ResolveInstance

        #region RegisterInstance

        [Test]
        public void RegisterInstance_CorrectlyRegistersSimpleInstance_WithNoKey()
        {
            double instance = 3.14d;

            Assert.DoesNotThrow(() => _container.RegisterInstance(typeof(double), instance));
        }

        [Test]
        public void RegisterInstance_CorrectlyRegistersSimpleInstance_WithKey()
        {
            const string Key = "Pi";
            double instance = 3.14d;

            Assert.DoesNotThrow(() => _container.RegisterInstance(typeof(double), instance, Key));
        }

        [Test]
        public void RegisterInstance_ThrowsException_WhenArgumentsInvalid()
        {
            Assert.Throws<ArgumentNullException>(
                () => _container.RegisterInstance(service: null, instance: null),
                $"Expected {nameof(ArgumentNullException)} when providing null for parameter @service!");
        }

        [Test]
        public void RegisterInstance_ThrowsException_WhenRegistrationAlreadyExistsNoKey()
        {
            _container.RegisterInstance(typeof(double), 42d);

            Assert.Throws<InvalidOperationException>(() => _container.RegisterInstance(typeof(double), 43d));
        }

        [Test]
        public void RegisterInstance_ThrowsException_WhenRegistrationAlreadyExistsWithSameKey()
        {
            Type service = typeof(int);
            const int Instance = 42;
            const string Key = "The answer";

            _container.RegisterInstance(service, Instance, Key);

            Assert.Throws<InvalidOperationException>(() => _container.RegisterInstance(service, Instance, Key));
        }

        [Test]
        public void RegisterInstance_SuccessfullyRegistersInstance_ForSameServiceWithDifferentKeys()
        {
            Type service = typeof(int);
            const int Instance = 42;
            const string KeyA = "The answer";
            const string KeyB = "Da answer";

            _container.RegisterInstance(service, Instance, KeyA);

            Assert.DoesNotThrow(() => _container.RegisterInstance(service, Instance, KeyB));
        }

        #endregion RegisterInstance

        #region RegisterSingleton

        [Test]
        public void RegisterSingleton_ThrowsException_WhenArgumentsInvalid()
        {
            Assert.Throws<ArgumentNullException>(
                () => _container.RegisterSingleton(service: null, typeof(User)),
                $"Expected {nameof(ArgumentNullException)} when providing null for parameter @service!");

            Assert.Throws<ArgumentNullException>(
                () => _container.RegisterSingleton(service: typeof(User), implementation: null),
                $"Expected {nameof(ArgumentNullException)} when providing null for parameter @implementation!");
        }

        [Test]
        public void RegisterSingleton_ThrowsException_WhenRegistrationAlreadyExistsNoKey()
        {
            _container.RegisterSingleton(typeof(User), typeof(User));

            Assert.Throws<InvalidOperationException>(() => _container.RegisterSingleton(typeof(User), typeof(User)));
        }

        [Test]
        public void RegisterSingleton_ThrowsException_WhenRegistrationAlreadyExistsWithSameKey()
        {
            const string Key = "User";

            _container.RegisterSingleton(typeof(User), typeof(User), Key);

            Assert.Throws<InvalidOperationException>(
                () => _container.RegisterSingleton(typeof(User), typeof(User), Key));
        }

        [Test]
        public void RegisterSingleton_RegistersInterfaceClassPair_Successfully()
        {
            Assert.DoesNotThrow(() => _container.RegisterSingleton(typeof(IUser), typeof(User)));
        }

        [Test]
        public void RegisterSingleton_RegistersClassClassPairSuccessfully()
        {
            Assert.DoesNotThrow(() => _container.RegisterSingleton(typeof(User), typeof(User)));
        }

        #endregion RegisterSingleton

        private interface IUser
        {
            string Name { get; }
        }

        private class User : IUser
        {
            public User(string name, Credentials credentials)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            }

            public User(string name, string username, string email)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Credentials = new Credentials(username, email);
            }

            public string Name { get; }
            public Credentials Credentials { get; }
        }

        private class Credentials
        {
            public Credentials(string username, string email)
            {
                Username = username ?? throw new ArgumentNullException(nameof(username));
                Email = email ?? throw new ArgumentNullException(nameof(email));
            }

            public string Username { get; }
            public string Email { get; }
        }
    }
}