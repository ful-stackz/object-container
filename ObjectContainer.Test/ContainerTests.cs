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

        [Test]
        public void RegisterInstance_CorrectlyRegistersSimpleInstance_WithNoKey()
        {
            double instance = 3.14d;

            _container.RegisterInstance(typeof(double), instance);

            Assert.AreEqual(instance, _container.ResolveInstance(typeof(double)));
        }

        [Test]
        public void RegisterInstance_CorrectlyRegistersSimpleInstance_WithKey()
        {
            const string Key = "Pi";
            double instance = 3.14d;

            _container.RegisterInstance(typeof(double), instance, Key);

            Assert.AreEqual(instance, _container.ResolveInstance(typeof(double), Key));
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
    }
}