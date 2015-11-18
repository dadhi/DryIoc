using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture][Explicit]
    public class GetInterfaceMapCustomImplTests
    {
        [Test]
        public void Find_method_implementing_requested_method_of_interface()
        {
            var implType = typeof(Boo<Boo<string>>);
            var methods = implType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var doMethods = methods.Where(m => m.Name == "Do" || m.Name.EndsWith(".Do")).ToArray();

            Assert.That(doMethods.Length, Is.EqualTo(1));
        }

        [Test]
        public void Find_method_implementing_requested_method_of_interface_Despite_other_overloads()
        {
            var implType = typeof(Voo);
            var methods = implType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var doMethods = methods.Where(m => (m.Name == "Do" || m.Name.EndsWith(".Do")) &&
                m.GetParameters().Length == 0).ToArray();

            Assert.That(doMethods.Length, Is.EqualTo(1));
        }

        [Test]
        public void Find_method_explicitly_implementing_requested_method_of_interface()
        {
            var implType = typeof(Coo);
            var methods = implType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var doMethods = methods.Where(m => m.Name == "Do" || m.Name.EndsWith(".Do")).ToArray();

            Assert.That(doMethods.Length, Is.EqualTo(2));
        }

        [Test]
        public void Find_method_explicitly_implementing_requested_method_of_Specific_interface()
        {
            var implType = typeof(Coo);
            var methods = implType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var doMethod = methods.FirstOrDefault(
                m => m.Name == "Do" || m.Name.EndsWith(".Do") 
                    && m.GetParameters().Length == 0 
                    && m.ReturnType == typeof(IBoo<string>).GetGenericParamsAndArgs()[0]);

            Assert.That(doMethod, Is.Not.Null);
        }
    }

    interface IBoo<T>
    {
        T Do();
    }

    class Boo<T> : IBoo<T>
    {
        public void My() { }

        public T Do()
        {
            return default(T);
        }
    }

    class Voo : IBoo<string>
    {
        public string Do(int count)
        {
            throw new NotImplementedException();
        }

        public string Do()
        {
            throw new NotImplementedException();
        }
    }

    class Coo : IBoo<int>, IBoo<string>
    {
        int IBoo<int>.Do()
        {
            throw new NotImplementedException();
        }

        string IBoo<string>.Do()
        {
            throw new NotImplementedException();
        }
    }
}
