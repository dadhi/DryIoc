using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Components.DictionaryAdapter;

namespace DryIoc.IssuesTests.MetadataProxies
{
    // Extension methods for typed metadata views generator using Castle Dictionary Adapter.
    public static class MetadataViewProxyGeneration
    {
        /// <summary>
        /// Adds the support for the MEF-like runtime-generated typed metadata views.
        /// </summary>
        /// <typeparam name="T">The type of the container</typeparam>
        /// <param name="registrator">The registrator.</param>
        public static T WithTypedMetadataViewGenerator<T>(this T registrator) where T : IRegistrator
        {
            // replace the standard MEF Lazy<,> wrapper with the magic one
            registrator.Register(typeof(Lazy<,>),
                made: _createLazyWithMetadataMethod,
                ifAlreadyRegistered: IfAlreadyRegistered.Replace,
                setup: Setup.WrapperWith(0));

            return registrator;
        }

        private static DictionaryAdapterFactory DictionaryAdapterFactory
        {
            get { return _dictionaryAdapterFactory ?? (_dictionaryAdapterFactory = new DictionaryAdapterFactory()); }
        }

        private static DictionaryAdapterFactory _dictionaryAdapterFactory;

        private static Lazy<T, TMetadata> CreateLazyWithMetadata<T, TMetadata>(Meta<Lazy<T>, IDictionary<string, object>> metaFactory)
        {
            if (metaFactory == null || metaFactory.Value == null)
                return null;

            if (typeof(TMetadata) == typeof(IDictionary<string, object>))
                return new Lazy<T, TMetadata>(() => metaFactory.Value.Value, (TMetadata)(metaFactory.Metadata));

            if (metaFactory.Metadata == null)
                return null;

            var metadata = metaFactory.Metadata.Values.OfType<TMetadata>().FirstOrDefault();
            if (metadata != null)
                return new Lazy<T, TMetadata>(() => metaFactory.Value.Value, metadata);

            if (!typeof(T).IsInterface)
                return null;

            var genMetadata = DictionaryAdapterFactory.GetAdapter<TMetadata, object>(metaFactory.Metadata);
            return new Lazy<T, TMetadata>(() => metaFactory.Value.Value, genMetadata);
        }

        private static Made _createLazyWithMetadataMethod { get; } = Made.Of(
            typeof(MetadataViewProxyGeneration).SingleMethod(nameof(CreateLazyWithMetadata), includeNonPublic: true));
    }
}
