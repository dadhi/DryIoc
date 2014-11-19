using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    class Issue85_SkipResolutionForPropertiesAndFieldsAlreadySetInConstructor
    {
        [Test]
        public void Only_not_assigned_properies_and_fields_should_be_resolved_So_that_assigned_field_value_should_Not_change()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(named: "another");
            container.Register<ClientWithAssignedProperty>(rules: PropertiesAndFields.Of.Name("Prop", serviceKey: "another"));

            var client = container.Resolve<ClientWithAssignedProperty>();

            Assert.That(client.Prop, Is.InstanceOf<Service>());
        }

        internal interface IService {}

        internal class Service : IService {}
        internal class AnotherService : IService {}

        public class ClientWithAssignedProperty
        {
            public IService Prop { get; set; }

            public ClientWithAssignedProperty(IService prop)
            {
                Prop = prop;
            }
        }
    }

    /// <summary>
    /// Posted as answer to: http://stackoverflow.com/questions/321650/how-do-i-set-a-field-value-in-an-c-sharp-expression-tree
    /// </summary>
    [TestFixture]
    public class CanSetPropAndFieldWithExpressionTreeInNet35
    {
        class Holder
        {
            public int Field;
            public string Prop { get; set; }
        }

        public static class FieldAndPropSetter
        {
            public static T SetField<T, TField>(T holder, ref TField field, TField value)
            {
                field = value;
                return holder;
            }

            public static T SetProp<T>(T holder, Action<T> setProp)
            {
                setProp(holder);
                return holder;
            }
        }

        [Test]
        public void Can_set_field_with_expression_tree_in_Net35()
        {
            // Shows how expression could look like:
            Func<Holder, Holder> setHolderField = h => FieldAndPropSetter.SetField(h, ref h.Field, 111);
            var holder = new Holder();
            holder = setHolderField(holder);
            Assert.AreEqual(111, holder.Field);

            var holderType = typeof(Holder);
            var field = holderType.GetField("Field");
            var fieldSetterMethod =
                typeof(FieldAndPropSetter).GetMethod("SetField")
                .MakeGenericMethod(holderType, field.FieldType);

            var holderParamExpr = Expression.Parameter(holderType, "h");
            var fieldAccessExpr = Expression.Field(holderParamExpr, field);

            // Result expression looks like: h => FieldAndPropSetter.SetField(h, ref h.Field, 222)
            var setHolderFieldExpr = Expression.Lambda<Func<Holder, Holder>>(
                Expression.Call(fieldSetterMethod, holderParamExpr, fieldAccessExpr, Expression.Constant(222)),
                holderParamExpr);

            var setHolderFieldGenerated = setHolderFieldExpr.Compile();
            holder = setHolderFieldGenerated(holder);
            Assert.AreEqual(222, holder.Field);
        }

        [Test]
        public void Can_set_property_with_expression_tree_in_Net35()
        {
            // Shows how expression could look like:
            Func<Holder, Holder> setHolderProp = h => FieldAndPropSetter.SetProp(h, _ => _.Prop = "ABC");
            var holder = new Holder();
            holder = setHolderProp(holder);
            Assert.AreEqual("ABC", holder.Prop);

            var holderType = typeof(Holder);
            var prop = holderType.GetProperty("Prop");
            var propSet = prop.GetSetMethod();

            var holderParamExpr = Expression.Parameter(holderType, "h");
            var callSetPropExpr = Expression.Call(holderParamExpr, propSet, Expression.Constant("XXX"));
            var setPropActionExpr = Expression.Lambda(callSetPropExpr, holderParamExpr);

            var propSetterMethod = typeof(FieldAndPropSetter).GetMethod("SetProp").MakeGenericMethod(holderType);

            // Result expression looks like: h => FieldAndPropSetter.SetProp(h, _ => _.Prop = "XXX")
            var setHolderPropExpr = Expression.Lambda<Func<Holder, Holder>>(
                Expression.Call(propSetterMethod, holderParamExpr, setPropActionExpr),
                holderParamExpr);

            var setHolderPropGenerated = setHolderPropExpr.Compile();
            holder = setHolderPropGenerated(holder);
            Assert.AreEqual("XXX", holder.Prop);
        }
    }
}
