using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Framework;
using Shared;

namespace Search
{
    public class CustomFormulaDecimalPart<T1, T2>
    {
        public CustomFormulaDecimalPart()
        {
        }

        public CustomFormulaDecimalPart(ICollection arg0, string arg1, bool arg2, AggregateFunction arg3)
        {
        }
    }

    public class SearchHelper
    {
        public SearchHelper(
        )
        {
        }
    }


    public class TranslationWithCode
        : ITranslationEntry
    {
        public TranslationWithCode(
            string arg0,
            string arg1,
            DictionaryCategory arg2
        )
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
        }

        public readonly string Field0;
        public readonly string Field1;
        public readonly DictionaryCategory Field2;
    }


    public class CustomFormulaSqlValue
    {
        public CustomFormulaSqlValue(
            string arg0,
            string arg1
        )
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly string Field0;
        public readonly string Field1;
    }


    public class FormulaPartFactory : Dictionary<string, string>
    {
        public FormulaPartFactory(
        )
        {
        }
    }


    public class CreateCustomFormulaPartDelegate
    {
        public CreateCustomFormulaPartDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly object Field0;
        public readonly IntPtr Field1;
    }


    public class CreateCustomFormulaParameterDelegate
    {
        public CreateCustomFormulaParameterDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly object Field0;
        public readonly IntPtr Field1;
    }


    public class AdditionFunction : CustomFormulaDecimalPart<TContext, AdditionFunction>
    {
        public AdditionFunction(
            ICollection arg0,
            string arg1,
            bool arg2,
            AggregateFunction arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
        }

        public readonly AggregateFunction Field4;
    }

    public class CustomFormulaDecimalPart<T>
    {
    }

    public class CollectionFunction : CustomFormulaPartBase<TContext>
    {
        public CollectionFunction(
        )
        {
        }

        public CollectionFunction(
            CustomFormulaPartBase<TContext> arg0
        )
        {
        }

        public CollectionFunction(
            CustomFormulaPartBase<TContext> arg0,
            CustomFormulaPartBase<TContext> arg1
        )
        {
            Field1 = arg1;
        }

        public CollectionFunction(
            ICollection arg0
        )
        {
        }

        public readonly CustomFormulaPartBase<TContext> Field1;
    }

    public class CustomFormulaPartBase<T>
    {
    }


    public class DivisionFunction : CustomFormulaDecimalPart<TContext, DivisionFunction>
    {
        public DivisionFunction(
            CustomFormulaDecimalPart<TContext> arg0,
            CustomFormulaDecimalPart<TContext> arg1,
            string arg2
        ) : base()
        {
        }
    }


    public class IsNullFunction : CustomFormulaDecimalPart<TContext, IsNullFunction>
    {
        public IsNullFunction(
        )
        {
        }

        public IsNullFunction(
            IsNullFunction arg0,
            AggregateFunction arg1
        ) : base()
        {
        }

        public IsNullFunction(
            CustomFormulaDecimalPart<TContext> arg0,
            CustomFormulaDecimalPart<TContext> arg1,
            string arg2,
            AggregateFunction arg3
        ) : base()
        {
        }
    }


    public class MultiplicationFunction : CustomFormulaDecimalPart<TContext, MultiplicationFunction>
    {
        public MultiplicationFunction(
        )
        {
        }

        public MultiplicationFunction(
            ICollection arg0,
            string arg1
        ) : base()
        {
        }

        public MultiplicationFunction(
            CustomFormulaDecimalPart<TContext> arg0,
            CustomFormulaDecimalPart<TContext> arg1,
            string arg2
        ) : base()
        {
        }
    }


    public class PercentageFunction : CustomFormulaDecimalPart<TContext, PercentageFunction>
    {
        public PercentageFunction(
            CustomFormulaDecimalPart<TContext> arg0,
            CustomFormulaDecimalPart<TContext> arg1,
            string arg2,
            bool arg3
        ) : base()
        {
        }
    }


    public class RangeFunction : CustomFormulaDecimalPart<TContext, RangeFunction>
    {
        public RangeFunction(
        )
        {
        }

        public RangeFunction(
            CustomFormulaDecimalPart<TContext> arg0,
            SortedSet<Decimal> arg1,
            string arg2,
            string arg3
        ) : base()
        {
        }
    }


    public class RoundingFunction : CustomFormulaDecimalPart<TContext, RoundingFunction>
    {
        public RoundingFunction(
        )
        {
        }

        public RoundingFunction(
            CustomFormulaDecimalPart<TContext> arg0,
            string arg1,
            int arg2
        ) : base()
        {
        }
    }


    public class SubtractionFunction : CustomFormulaDecimalPart<TContext, SubtractionFunction>
    {
        public SubtractionFunction(
        )
        {
        }

        public SubtractionFunction(
            CustomFormulaDecimalPart<TContext> arg0,
            CustomFormulaDecimalPart<TContext> arg1,
            string arg2,
            bool arg3
        ) : base()
        {
        }
    }


    public interface ICustomFormulaPart
    {
    }


    public interface IFormulaHandlerParentHandler
    {
    }


    public interface ISqlFormulaHandler
        : ICustomFormulaHandler
    {
    }


    public class CustomFormulaBooleanPart
    {
        public CustomFormulaBooleanPart(
            Phrase arg0,
            string arg2
        ) : base()
        {
            Field0 = arg0;
            Field2 = arg2;
        }

        public readonly Phrase Field0;
        public readonly string Field2;
    }


    public class CustomFormulaDecimalPart : CustomFormulaDecimalPart<TContext>
    {
        public CustomFormulaDecimalPart(
            Phrase arg0,
            string arg2,
            AggregateFunction arg3
        ) : base()
        {
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly string Field2;
        public readonly AggregateFunction Field3;
    }


    public class CustomFormulaGenericDecimalPart : CustomFormulaDecimalPart<TContext, CustomFormulaGenericPartParameter>
    {
        public CustomFormulaGenericDecimalPart(
            ICollection arg0,
            string arg1,
            bool arg2,
            AggregateFunction arg3,
            SearchFieldDataType arg4,
            AggregateFunction arg5,
            string arg6
        ) : base(arg0, arg1, arg2, arg3)
        {
            Field4 = arg4;
            Field5 = arg5;
            Field6 = arg6;
        }

        public readonly SearchFieldDataType Field4;
        public readonly AggregateFunction Field5;
        public readonly string Field6;
    }


    public class CustomFormulaPartBase
        : ICustomFormulaPart
    {
        public CustomFormulaPartBase(
            AggregateFunction arg0
        )
        {
            Field0 = arg0;
        }

        public readonly AggregateFunction Field0;
    }


    public class CustomFormulaStringPart<TContext> : CustomFormulaPartBase<TContext>
    {
        public CustomFormulaStringPart(
        )
        {
        }
    }


    public class CustomFormulaStringPart : CustomFormulaStringPart<TContext>
    {
        public CustomFormulaStringPart(
            Phrase arg0,
            string arg2
        ) : base()
        {
            Field0 = arg0;
            Field2 = arg2;
        }

        public readonly Phrase Field0;
        public readonly string Field2;
    }

    public class CustomFormulaXmlPart<T, T1>
    {
        public CustomFormulaXmlPart(
            Phrase arg0,
            string arg2
        ) : base()
        {
            Field0 = arg0;
            Field2 = arg2;
        }

        public readonly Phrase Field0;
        public readonly string Field2;

        protected CustomFormulaXmlPart()
        {
        }
    }


    public class CommonSearchCriteria : SearchCriteriaBase
    {
        public CommonSearchCriteria(
        )
        {
        }

        public CommonSearchCriteria(
            ICollection<SearchRequestCriteriaField> arg0
        ) : base()
        {
            Field0 = arg0;
        }

        public readonly ICollection<SearchRequestCriteriaField> Field0;
    }


    public class SearchDefinitionBase
        : ISearchDefinition
    {
        public SearchDefinitionBase(
            ICollection arg0
        )
        {
            Field0 = arg0;
        }

        public readonly ICollection Field0;
    }


    public class FormulaFieldDefinition<TContext, TSearchCriteria> : ISqlSearchFieldDefinition<TContext, TSearchCriteria>, ISqlSearchFieldRegisterEntry<TContext, TSearchCriteria>
        where TContext : IContext
        where TSearchCriteria : ISearchCriteria
    {

        private string _identifier;
        protected CustomFormulaPartBase<TContext> Formula;
        private CheckUserRightsDelegate<TContext> _isAccessibleByUser;
        private SearchFieldOption _options;
        private SearchCriteriaComparison _defaultComparison;
        private string _criteriaPlacing;

        public InitializeSqlDelegate<TContext, TSearchCriteria> InitializeSqlDelegate;

        public FormulaFieldDefinition(string identifier, CustomFormulaPartBase<TContext> formula)
        {
        }
    }



    public class CreateCustomFormulaDelegate
    {
        public CreateCustomFormulaDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly object Field0;
        public readonly IntPtr Field1;
    }


    public class FormulaFieldRegisterEntry
        : ISqlSearchFieldRegisterEntry
    {
        public FormulaFieldRegisterEntry(
            string arg0,
            SearchFieldDataType arg2,
            string arg3,
            bool arg5
        )
        {
            Field0 = arg0;
            Field2 = arg2;
            Field3 = arg3;
            Field5 = arg5;
        }

        public readonly string Field0;
        public readonly SearchFieldDataType Field2;
        public readonly string Field3;
        public readonly bool Field5;
    }


    public class AddSearchFieldDelegate
    {
        public AddSearchFieldDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly object Field0;
        public readonly IntPtr Field1;
    }

    public interface ICriteriaBlock
    {
    }

    public interface IExtendedDataReader
        : Framework.IExtendedDataReader
    {
    }


    public interface IInnerSearchFieldDefinition
        : ISearchFieldDefinition
    {
    }


    public interface ISqlExpressionModifier<TContext>
    {
    }


    public class InitializeSqlDelegate
    {
        public InitializeSqlDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly object Field0;
        public readonly IntPtr Field1;
    }


    public class SearchFieldTempInfo
        : ISearchFieldDefinition
    {
        public SearchFieldTempInfo(
            SearchRequestField arg0
        )
        {
        }

        public readonly SearchRequestField Field0;
    }


    public delegate void InitializeSqlDelegate<TContext, TSearchCriteria>(TContext context, IDbCommand command, System.Text.StringBuilder str, SearchFieldTempInfo<TContext, TSearchCriteria> fieldInfo)
        where TContext : IContext
        where TSearchCriteria : ISearchCriteria;

    public partial interface ISqlSearchFieldDefinition<TContext, TSearchCriteria> : ISearchFieldDefinition
        where TContext : IContext
        where TSearchCriteria : ISearchCriteria
    {
    }


    public interface ISqlSearchFieldRegisterEntry<TContext, TSearchCriteria> : ISearchFieldDefinition
        where TContext : IContext
        where TSearchCriteria : ISearchCriteria
    {
    }


    public class SearchDataReader : DataReader
        , Framework.IExtendedDataReader
    {
        public SearchDataReader(
            TContext arg0,
            SqlDataReader arg1,
            ISearchRequest arg2
        ) : base()
        {
            Field1 = arg1;
            Field2 = arg2;
        }

        public readonly SqlDataReader Field1;
        public readonly ISearchRequest Field2;
    }


    public class SearchFactory
    {
        public SearchFactory(
            IContextService<TContext> arg0,
            IDatabaseConnionResolver<TContext> arg1
        )
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly IContextService<TContext> Field0;
        public readonly IDatabaseConnionResolver<TContext> Field1;
    }


    public partial class SearchFieldDefinition
        : ISqlSearchFieldDefinition<TContext, SearchCriteria>, ISqlSearchFieldRegisterEntry
    {
        public SearchFieldDefinition(
            string arg0,
            SearchFieldDataType arg1,
            string arg2,
            string arg3,
            string arg4,
            CheckUserRightsDelegate<TContext> arg5,
            ISqlExpressionModifier<TContext> arg6,
            SearchFieldOption arg7,
            SearchCriteriaComparison arg8,
            AggregateFunction arg9,
            string arg10,
            string[] arg11
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
            Field4 = arg4;
            Field5 = arg5;
            Field6 = arg6;
            Field7 = arg7;
            Field8 = arg8;
            Field9 = arg9;
            Field10 = arg10;
            Field11 = arg11;
        }

        public readonly string Field0;
        public readonly SearchFieldDataType Field1;
        public readonly string Field2;
        public readonly string Field3;
        public readonly string Field4;
        public readonly CheckUserRightsDelegate<TContext> Field5;
        public readonly ISqlExpressionModifier<TContext> Field6;
        public readonly SearchFieldOption Field7;
        public readonly SearchCriteriaComparison Field8;
        public readonly AggregateFunction Field9;
        public readonly string Field10;
        public readonly string[] Field11;
    }

    internal interface ISqlSearchFieldRegisterEntry
    {
    }

    public partial class SearchDefinitionBase<TContext, TSearchCriteria>
        where TContext : IContext
        where TSearchCriteria : ISearchCriteria
    {
        public class XmlSearchFieldDefinitionOld : SearchFieldDefinition<TContext, TSearchCriteria>,
            IInnerSearchFieldDefinition<TContext, TSearchCriteria>
        {
            public XmlSearchFieldDefinitionOld(string identifier, string sqlExpression, string sortExpression,
                GetSqlExpressionDelegate getInnerSqlExpression,
                CheckUserRightsDelegate<TContext> isAccessibleByUser = null,
                ISqlExpressionModifier<TContext> accessRightsModifier = null) : base()
            {
            }
        }
    }

    internal interface IInnerSearchFieldDefinition<TContext, TSearchCriteria>
        where TContext : IContext
        where TSearchCriteria : ISearchCriteria
    {
    }

    public class SearchFieldDefinition<TContext, TSearchCriteria> : ISqlSearchFieldDefinition<TContext, TSearchCriteria>, ISqlSearchFieldRegisterEntry<TContext, TSearchCriteria>
        where TContext : IContext
        where TSearchCriteria : ISearchCriteria
    {
        private string _arg0;
        private string _arg1;
        private Phrase _arg2;
        private Phrase _arg3;
        private string _arg4;
        private CheckUserRightsDelegate<Shared.TContext> _arg5;
        private ISqlExpressionModifier<Shared.TContext> _arg6;

        public SearchFieldDefinition(string arg0, string arg1, Phrase arg2, Phrase arg3, string arg4,
            CheckUserRightsDelegate<Shared.TContext> arg5, ISqlExpressionModifier<Shared.TContext> arg6)
        {
            _arg0 = arg0;
            _arg1 = arg1;
            _arg2 = arg2;
            _arg3 = arg3;
            _arg4 = arg4;
            _arg5 = arg5;
            _arg6 = arg6;
        }

        protected SearchFieldDefinition(string s, string s1, ICollection<TranslationWithCode> translationWithCodes,
            bool b, string s2, CheckUserRightsDelegate<Shared.TContext> checkUserRightsDelegate,
            ISqlExpressionModifier<Shared.TContext> sqlExpressionModifier)
        {
        }

        protected SearchFieldDefinition(bool? b)
        {
        }

        protected SearchFieldDefinition(string s, SearchFieldDataType s1, string translationWithCodes, string s2,
            string s3, CheckUserRightsDelegate<Shared.TContext> checkUserRightsDelegate,
            ISqlExpressionModifier<Shared.TContext> sqlExpressionModifier, SearchFieldOption arg7,
            SearchCriteriaComparison arg8, AggregateFunction arg9, string arg10, string[] arg11)
        {
        }

        public SearchFieldDefinition()
        {
        }
    }

    public class ModifySqlExpressionDelegate
    {
        public ModifySqlExpressionDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly object Field0;
        public readonly IntPtr Field1;
    }


    public class AdjustTimePeriodDelegate
    {
        public AdjustTimePeriodDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly object Field0;
        public readonly IntPtr Field1;
    }


    public class BindValue
    {
        public BindValue(
            string arg0
        )
        {
            Field0 = arg0;
        }

        public readonly string Field0;
    }


    public class ContactNameTranslatedSearchFieldDefinition : SearchFieldDefinition<TContext, SearchCriteria>
    {
        public ContactNameTranslatedSearchFieldDefinition(string arg0, string arg1, Phrase arg2, Phrase arg3,
            string arg4, CheckUserRightsDelegate<TContext> arg5, ISqlExpressionModifier<TContext> arg6) : base(arg0,
            arg1, arg2, arg3, arg4, arg5, arg6)
        {
        }

        protected ContactNameTranslatedSearchFieldDefinition(string s, string s1,
            ICollection<TranslationWithCode> translationWithCodes, bool b, string s2,
            CheckUserRightsDelegate<TContext> checkUserRightsDelegate,
            ISqlExpressionModifier<TContext> sqlExpressionModifier) : base(s, s1, translationWithCodes, b, s2,
            checkUserRightsDelegate, sqlExpressionModifier)
        {
        }
    }

    public class SearchCriteria : ISearchCriteria
    {
    }

    public class SearchFieldTempInfo<TContext, TSearchCriteria> : ISearchFieldDefinition
        where TContext : IContext
        where TSearchCriteria : ISearchCriteria
    {
        public SearchRequestField RequestField;
        public readonly ISearchRequest<TSearchCriteria> SearchRequest;
        public readonly ISearchDefinition<TContext, TSearchCriteria> SearchDefinition;
        public readonly ISqlSearchFieldDefinition<TContext, TSearchCriteria> SearchField;
        private Dictionary<string, object> _values;

        public SearchFieldTempInfo(SearchRequestField requestField, ISearchRequest<TSearchCriteria> searchRequest,
            ISearchDefinition<TContext, TSearchCriteria> searchDefinition,
            ISqlSearchFieldDefinition<TContext, TSearchCriteria> searchField)
        {
            RequestField = requestField;
            SearchRequest = searchRequest;
            SearchDefinition = searchDefinition;
            SearchField = searchField;
        }
    }

    public partial interface ISqlSearchFieldDefinition<TContext, TSearchCriteria>
        where TContext : IContext
        where TSearchCriteria : ISearchCriteria
    {
    }

    public interface ISearchDefinition<TContext, TSearchCriteria>
        where TContext : IContext
        where TSearchCriteria : ISearchCriteria
    {
    }

    public class EntityAccessInfoListFieldDefinition<TContext, TSearchCriteria, TEntity> :
        SearchFieldDefinition<TContext, TSearchCriteria>, IInnerSearchFieldDefinition<TContext, TSearchCriteria>
        where TContext : IContext
        where TSearchCriteria : SearchCriteriaBase
        where TEntity : IEntityAccessInfo<TContext>
    {
        public delegate string GetSqlExpressionDelegate(TContext context,
            SearchFieldTempInfo<TContext, TSearchCriteria> fieldInfo);

        private readonly GetSqlExpressionDelegate _getInnerSqlExpression;

        public EntityAccessInfoListFieldDefinition(string identifier, string sqlExpression, string sortExpression,
            GetSqlExpressionDelegate getInnerSqlExpression = null,
            CheckUserRightsDelegate<TContext> isAccessibleByUser = null,
            ISqlExpressionModifier<TContext> accessRightsModifier = null, SearchFieldDataType dataType = null
        ) : base()
        {
            _getInnerSqlExpression = getInnerSqlExpression;
        }

        protected EntityAccessInfoListFieldDefinition()
        {
        }

        protected EntityAccessInfoListFieldDefinition(string identifier)
        {
        }
    }


    public class InnerSearchFieldDefinition : SearchFieldDefinition<TContext, SearchCriteria>
        , IInnerSearchFieldDefinition
    {
        public InnerSearchFieldDefinition(
            string arg0,
            string arg1,
            string arg2,
            CheckUserRightsDelegate<TContext> arg4,
            ISqlExpressionModifier<TContext> arg5,
            SearchFieldOption arg6,
            SearchCriteriaComparison arg7,
            AggregateFunction arg8,
            SearchFieldDataType arg9
        ) : base()
        {
        }

        public InnerSearchFieldDefinition(
            string arg0,
            string arg1,
            string arg2,
            string arg3,
            CheckUserRightsDelegate<TContext> arg4,
            ISqlExpressionModifier<TContext> arg5,
            SearchFieldOption arg6,
            SearchCriteriaComparison arg7,
            AggregateFunction arg8,
            SearchFieldDataType arg9
        ) : base()
        {
        }
    }


    public class MultipliedSearchFieldDefinition : SearchFieldDefinition<TContext, SearchCriteria>
    {
        public MultipliedSearchFieldDefinition(
            string arg0
        ) : base()
        {
        }
    }


    public class RatioSearchFieldDefinition : SearchFieldDefinition<TContext, SearchCriteria>
    {
        public RatioSearchFieldDefinition(
            string arg0,
            string arg1,
            string arg2,
            SearchFieldDataType arg3,
            string arg4,
            string arg5,
            CheckUserRightsDelegate<TContext> arg6,
            ISqlExpressionModifier<TContext> arg7,
            SearchFieldOption arg8,
            SearchCriteriaComparison arg9
        ) : base()
        {
        }
    }


    public class SalutationTranslatedSearchFieldDefinition : SearchFieldDefinition<TContext, SearchCriteria>
    {
        public SalutationTranslatedSearchFieldDefinition(
            string arg0,
            string arg1
        ) : base()
        {
        }
    }


    public class GetSortFieldsDelegate
    {
        public GetSortFieldsDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly object Field0;
        public readonly IntPtr Field1;
    }


    public class ReadEntityDelegate
    {
        public ReadEntityDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly object Field0;
        public readonly IntPtr Field1;
    }


    public class CriteriaBlock
        : ICriteriaBlock
    {
        public CriteriaBlock(
            SearchDefinitionBase<TContext, SearchCriteria> arg0,
            string arg1,
            string arg2,
            string arg3
        )
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly SearchDefinitionBase<TContext, SearchCriteria> Field0;
        public readonly string Field1;
        public readonly string Field2;
        public readonly string Field3;
    }


    public partial class SearchFieldDefinition : SearchFieldDefinition<TContext, SearchCriteria>
    {
        public SearchFieldDefinition(
            string arg0,
            SearchFieldDataType arg1,
            string arg2,
            string arg3,
            string arg4,
            CheckUserRightsDelegate<TContext> arg5,
            ISqlExpressionModifier<TContext> arg6
        ) : base()
        {
        }
    }


    public class EnumSearchFieldDefinition : SearchFieldDefinition<TContext, SearchCriteria>
    {
        public EnumSearchFieldDefinition(
            string arg0,
            SearchFieldDataType arg1,
            string arg2,
            string arg3,
            string arg4,
            CheckUserRightsDelegate<TContext> arg5,
            ISqlExpressionModifier<TContext> arg6,
            SearchFieldOption arg7,
            SearchCriteriaComparison arg8,
            AggregateFunction arg9,
            string arg10,
            string[] arg11
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11)
        {
        }
    }


    public class UtcDateComparisonSearchFieldDefinition : SearchFieldDefinition<TContext, SearchCriteria>
    {
        public UtcDateComparisonSearchFieldDefinition(
            string arg0,
            string arg1,
            string arg2,
            string arg3,
            string arg4,
            CheckUserRightsDelegate<TContext> arg5,
            ISqlExpressionModifier<TContext> arg6,
            SearchFieldOption arg7,
            SearchCriteriaComparison arg8,
            AggregateFunction arg9
        ) : base()
        {
        }
    }


    public class FieldContainer : Dictionary<string, string>
    {
        public FieldContainer(
            IExtendedDataReader<TContext, SearchCriteria> arg0
        ) : base()
        {
        }
    }

    public interface IExtendedDataReader<TContext> : IExtendedDataReader where TContext : IContext
    {
    }

    public interface IExtendedDataReader<TContext, TSearchCriteria> : IExtendedDataReader<TContext>
        where TContext : IContext
        where TSearchCriteria : ISearchCriteria
    {
        new ISearchRequest<TSearchCriteria> Request { get; set; }

        IDict Fields { get; }
    }

    public class DataReaderNamedPropertyContainer
        : INamedPropertyContainer
    {
        public DataReaderNamedPropertyContainer(
            TContext arg0,
            IExtendedDataReader<TContext, SearchCriteria> arg1
        )
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly TContext Field0;
        public readonly IExtendedDataReader<TContext, SearchCriteria> Field1;
    }


    public class SearchFieldRegisterEntry
        : ISqlSearchFieldRegisterEntry
    {
        public SearchFieldRegisterEntry(
            string arg0,
            CreateSearchFieldDelegate<TContext, SearchCriteria> arg1,
            SearchFieldDataType arg2,
            CheckUserRightsDelegate<TContext> arg3,
            bool arg4
        )
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
            Field4 = arg4;
        }

        public readonly string Field0;
        public readonly CreateSearchFieldDelegate<TContext, SearchCriteria> Field1;
        public readonly SearchFieldDataType Field2;
        public readonly CheckUserRightsDelegate<TContext> Field3;
        public readonly bool Field4;
    }


    public class TranslatedBooleanSearchFieldDefinition : SearchFieldDefinition<TContext, SearchCriteria>
    {
        public TranslatedBooleanSearchFieldDefinition(
            string arg0,
            string arg1,
            Phrase arg2,
            Phrase arg3,
            string arg4,
            CheckUserRightsDelegate<TContext> arg5,
            ISqlExpressionModifier<TContext> arg6
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6)
        {
        }
    }


    public class TranslatedSearchFieldDefinition : SearchFieldDefinition<TContext, SearchCriteria>
    {
        public TranslatedSearchFieldDefinition(
            string arg0,
            string arg1,
            ICollection<TranslationWithCode> arg2,
            bool arg3,
            string arg4,
            CheckUserRightsDelegate<TContext> arg5,
            ISqlExpressionModifier<TContext> arg6
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6)
        {
        }
    }

    public partial class SearchDefinitionBase<TContext, TSearchCriteria>
        where TContext : IContext
        where TSearchCriteria : ISearchCriteria
    {
        public class TypeAndIdSearchFieldDefinition : ISqlSearchFieldDefinition<TContext, TSearchCriteria>,
            ISqlSearchFieldRegisterEntry<TContext, TSearchCriteria>
        {
            private readonly string _identifier;
            private readonly CheckUserRightsDelegate<TContext> _isAccessibleByUser;
            private readonly ISqlExpressionModifier<TContext> _accessRightsModifier;
            private readonly SearchFieldOption _options;
            private readonly string _criteriaPlacing;
            private readonly string[] _tablesToJoin;
            private readonly string _sqlExpressionForType;
            private readonly string _sqlExpressionForId;

            public TypeAndIdSearchFieldDefinition(string identifier, string sqlExpressionForType,
                string sqlExpressionForId,
                string criteriaPlacing,
                CheckUserRightsDelegate<TContext> isAccessibleByUser,
                ISqlExpressionModifier<TContext> accessRightsModifier,
                SearchFieldOption options,
                string[] tablesToJoin = null
            )
            {
                _identifier = identifier;
                _sqlExpressionForType = sqlExpressionForType;
                _sqlExpressionForId = sqlExpressionForId;
                _isAccessibleByUser = isAccessibleByUser;
                _options = options;
                _accessRightsModifier = accessRightsModifier;
                _criteriaPlacing = criteriaPlacing;
                _tablesToJoin = tablesToJoin;
            }
        }
    }


    public class UserNameTranslatedSearchFieldDefinition : SearchFieldDefinition<TContext, SearchCriteria>
    {
        public UserNameTranslatedSearchFieldDefinition(
            string arg0,
            string arg1
        ) : base()
        {
        }
    }


    public class WeekBeginSearchFieldDefinition : SearchFieldDefinition<TContext, SearchCriteria>
    {
        public WeekBeginSearchFieldDefinition(
            string arg0,
            string arg1,
            string arg2,
            CheckUserRightsDelegate<TContext> arg3,
            ISqlExpressionModifier<TContext> arg4,
            SearchFieldOption arg5,
            string arg6
        ) : base()
        {
        }
    }


    public class XmlSearchFieldDefinition<TT, TT2> : SearchFieldDefinition<TContext, SearchCriteria>
        , IInnerSearchFieldDefinition
    {
        public XmlSearchFieldDefinition(
            string arg0,
            string arg1,
            string arg2,
            GetSqlExpressionDelegate<TContext, SearchCriteria> arg3,
            CheckUserRightsDelegate<TContext> arg4,
            ISqlExpressionModifier<TContext> arg5
        ) : base()
        {
        }
    }


    public class XmlSearchFieldDefinitionOld : SearchFieldDefinition<TContext, SearchCriteria>
        , IInnerSearchFieldDefinition
    {
        public XmlSearchFieldDefinitionOld(
            string arg0,
            string arg1,
            string arg2,
            GetSqlExpressionDelegate<TContext, SearchCriteria> arg3,
            CheckUserRightsDelegate<TContext> arg4,
            ISqlExpressionModifier<TContext> arg5
        ) : base()
        {
        }
    }


    public class XmlStorableSearchFieldDefinition : XmlSearchFieldDefinition<TContext, SearchCriteria>
    {
        public XmlStorableSearchFieldDefinition(
            string arg0,
            string arg1,
            string arg2,
            GetSqlExpressionDelegate<TContext, SearchCriteria> arg3,
            CheckUserRightsDelegate<TContext> arg4,
            ISqlExpressionModifier<TContext> arg5
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5)
        {
        }
    }


    public class JoinedTable
    {
        public JoinedTable(
        )
        {
        }
    }


    public class UtcTimePeriod
    {
        public UtcTimePeriod(
            TimeZoneInfo arg0,
            DateTime? arg1,
            DateTime? arg2
        )
        {
            Field0 = arg0;
            Field2 = arg2;
        }

        public UtcTimePeriod(
            TimeZoneInfo arg0,
            TimePeriod arg1
        )
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly TimeZoneInfo Field0;
        public readonly TimePeriod Field1;
        public readonly DateTime? Field2;
    }


    public class GetSqlExpressionDelegate
    {
        public GetSqlExpressionDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly object Field0;
        public readonly IntPtr Field1;
    }

    public class GetSqlExpressionDelegate<TT, TT2> : GetSqlExpressionDelegate
    {
        public GetSqlExpressionDelegate(object arg0, IntPtr arg1) : base(arg0, arg1)
        {
        }
    }


    public class ReadFieldInfo
    {
        public ReadFieldInfo(
        )
        {
        }
    }


    public class CreateSearchFieldDelegate
    {
        public CreateSearchFieldDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly object Field0;
        public readonly IntPtr Field1;
    }

    public class CreateSearchFieldDelegate<T, T2> : CreateSearchFieldDelegate
    {
        public CreateSearchFieldDelegate(
            object arg0,
            IntPtr arg1
        ) : base(arg0, arg1)
        {
        }
    }
}