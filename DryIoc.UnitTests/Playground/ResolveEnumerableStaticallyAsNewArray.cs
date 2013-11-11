using System.Collections.Generic;
using System.Linq.Expressions;

namespace DryIoc.UnitTests.Playground
{
    public class ResolveEnumerableStaticallyAsNewArray
    {
        public static ResolutionRules.ResolveUnregisteredService TryResolveEnumerableOrArrayStatically = (req, _) =>
        {
            if (!req.ServiceType.IsArray && req.OpenGenericServiceType != typeof(IEnumerable<>))
                return null;

            return new DelegateFactory((request, registry) =>
            {
                var collectionType = request.ServiceType;

                var itemType = collectionType.IsArray
                    ? collectionType.GetElementType()
                    : collectionType.GetGenericArguments()[0];

                var wrappedItemType = registry.GetWrappedServiceTypeOrSelf(itemType);

                // Composite pattern support: filter out composite root from available keys.
                var parent = request.GetNonWrapperParentOrNull();
                var itemKeys = parent != null && parent.ServiceType == wrappedItemType
                    ? registry.GetKeys(wrappedItemType, factory => factory.ID != parent.FactoryID)
                    : registry.GetKeys(wrappedItemType);

                //Throw.If(itemKeys.Length == 0, NO_REGISTERED_SERVICES_OF_ITEM_TYPE_FOUND, unwrappedItemType);

                var itemExpressions = new List<Expression>();
                foreach (var itemKey in itemKeys)
                {
                    var itemRequest = request.Push(itemType, itemKey);
                    var itemFactory = registry.GetOrAddFactory(itemRequest, IfUnresolved.ReturnNull);
                    if (itemFactory != null)
                        itemExpressions.Add(itemFactory.GetExpression(itemRequest, registry));
                }

                //Throw.If(itemExpressions.Count == 0, UNABLE_TO_RESOLVE_ANY_SERVICE_OF_ITEM_TYPE, itemType);
                return Expression.NewArrayInit(itemType.ThrowIfNull(), itemExpressions);
            },
            setup: ServiceSetup.With(FactoryCachePolicy.NotCacheExpression));
        };

        private static readonly string NO_REGISTERED_SERVICES_OF_ITEM_TYPE_FOUND =
            "There is no registered services of item type {0}.";

        private static readonly string UNABLE_TO_RESOLVE_ANY_SERVICE_OF_ITEM_TYPE =
            "Unable to resolve any service of item type {0}.";
    }
}
