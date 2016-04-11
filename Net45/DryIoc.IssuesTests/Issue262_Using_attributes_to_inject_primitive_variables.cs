using System;
using System.Reflection;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue262_Using_attributes_to_inject_primitive_variables
    {
        [Test]
        public void Test_parameter()
        {
            var container = new Container(rules => rules
                .WithUnknownServiceResolvers(request => new DelegateFactory(resolver =>
                {
                    if (request.ServiceType == typeof(string))
                        return request.Is(parameter: p =>
                        {
                            var attribute = p.GetCustomAttribute<ServiceKeyResolverAttribute>();
                            return attribute == null ? null : ConfigWrapper.GetValue(attribute.Key);
                        });

                    return null;
                })));

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
        public void Test_field()
        {
            var container = new Container(rules => rules
                .With(propertiesAndFields: PropertiesAndFields.Of.Name("BaseUrlField").Name("BaseUrlProperty"))
                .WithUnknownServiceResolvers(request => new DelegateFactory(resolver =>
                {
                    if (request.ServiceType == typeof(string))
                        return request.Is(
                            field: f =>
                            {
                                var attribute = f.GetCustomAttribute<ServiceKeyResolverAttribute>();
                                return attribute == null ? null : ConfigWrapper.GetValue(attribute.Key);
                            },
                            property: p =>
                            {
                                var attribute = p.GetCustomAttribute<ServiceKeyResolverAttribute>();
                                return attribute == null ? null : ConfigWrapper.GetValue(attribute.Key);
                            });

                    return null;
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
                    new DelegateFactory(resolver => 
                        request.ServiceType == typeof(string) ? request.Is(() => "x") : null)));

            var service = container.Resolve<string>();

            Assert.AreEqual("x", service);
        }
    }
}
