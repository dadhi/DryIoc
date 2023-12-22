using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue581_Constructor_injection_with_array_parameter : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var container = new Container();
            container.Register<User>(made: Parameters.Of
                .Name("tags", defaultValue: new[] { "123", "456" })
                .Name("name", defaultValue: "mike"));

            var user = container.Resolve<User>();

            Assert.AreEqual("mike", user.Name);

            //failed,tags.lenght==0
            Assert.AreEqual(2, user.Tags.Length);
        }

        public class User
        {
            public User(string name, string[] tags)
            {
                Name = name;
                Tags = tags;
            }

            public string Name { get; }

            public string[] Tags { get; }
        }
    }
}
