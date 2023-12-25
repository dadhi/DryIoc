using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue262_Using_attributes_to_inject_primitive_variables : ITest
    {
        public int Run()
        {
            Test_parameter();
            Test_field_2();
            Test_root();
            return 3;
        }

        [Test]
        public void Test_parameter()
        {
            var container = new Container(rules => rules
                .With(parameters: request => p =>
                {
                    var attr = p.GetCustomAttribute<ServiceKeyResolverAttribute>();
                    return attr == null ? null
                        : ParameterServiceInfo.Of(p)
                            .WithDetails(ServiceDetails.Of(ConfigWrapper.GetValue(attr.Key)));
                }));

            container.Register<IService, Service>();
            var service = (Service)container.Resolve<IService>();

            Assert.AreEqual("id: paths.baseUrl", service.BaseUrl);
        }

        public static class ConfigWrapper
        {
            public static object GetValue(object key)
            {
                return "id: " + key;
            }
        }

        public class ServiceKeyResolverAttribute : Attribute
        {
            public string Key { get; set; }

            public ServiceKeyResolverAttribute(string key)
            {
                Key = key;
            }
        }

        public interface IService { }

        public class Service : IService
        {
            public string BaseUrl { get; private set; }

            public Service([ServiceKeyResolver("paths.baseUrl")]string baseUrl)
            {
                BaseUrl = baseUrl;
            }
        }

        [Test]
        public void Test_field_2()
        {
            var container = new Container(rules => rules
                .With(propertiesAndFields: request => 
                    request.ImplementationType.GetMembers(t =>
                        t.DeclaredFields.Cast<MemberInfo>().Concat(
                        t.DeclaredProperties.Cast<MemberInfo>()))
                    .Select(member =>
                    {
                        var attr = member.GetCustomAttribute<ServiceKeyResolverAttribute>();
                        if (attr == null)
                            return null;
                        var value = ConfigWrapper.GetValue(attr.Key);
                        return PropertyOrFieldServiceInfo.Of(member).WithDetails(ServiceDetails.Of(value));
                    })));

            container.Register<IService, BlahService>();
            var service = (BlahService)container.Resolve<IService>();

            Assert.AreEqual("id: paths.baseUrl", service.BaseUrlField);
            Assert.AreEqual("id: paths.baseUrl", service.BaseUrlProperty);
        }

        public class BlahService : IService
        {
            [ServiceKeyResolver("paths.baseUrl")]
            public string BaseUrlField;

            [ServiceKeyResolver("paths.baseUrl")]
            public string BaseUrlProperty { get; set; }
        }

        [Test]
        public void Test_root()
        {
            var container = new Container(rules => rules
                .WithUnknownServiceResolvers(request => 
                    DelegateFactory.Of(resolver => request.ServiceType == typeof(string) ? request.Is(() => "x") : null)));

            var service = container.Resolve<string>();

            Assert.AreEqual("x", service);
        }
    }
}
