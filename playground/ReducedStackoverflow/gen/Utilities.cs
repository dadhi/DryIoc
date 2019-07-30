using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Utilities
{
    public class Strings
    {
    }


    public class DisplayFormatAttribute : Attribute
    {
        public DisplayFormatAttribute(
        )
        {
        }

        public DisplayFormatAttribute(
            string arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly string field0;
    }


    public class JsonFormatAttribute : Attribute
    {
        public JsonFormatAttribute(
        )
        {
        }
    }


    public class CsvParser
    {
        public CsvParser(
            StreamReader arg0,
            bool arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly StreamReader field0;
        public readonly bool field1;
    }


    public class CsvWriter
    {
        public CsvWriter(
            TextWriter arg0
        )
        {
            field0 = arg0;
        }

        public readonly TextWriter field0;
    }


    public class EventLog
    {
        public EventLog(
        )
        {
        }
    }


    public class FixedWidthWriter
    {
        public FixedWidthWriter(
            BinaryWriter arg0,
            Encoding arg1,
            ColumnInfo[] arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public FixedWidthWriter(
            BinaryWriter arg0,
            Encoding arg1,
            int[] arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public FixedWidthWriter(
            BinaryWriter arg0,
            Encoding arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly BinaryWriter field0;
        public readonly Encoding field1;
        public readonly int[] field2;
    }


    public class GeoIPLookup
    {
        public GeoIPLookup(
            string arg0
        )
        {
            field0 = arg0;
        }

        public readonly string field0;
    }


    public class HashPath
    {
        public HashPath(
        )
        {
        }
    }


    public class HtmlEncode
    {
        public HtmlEncode(
        )
        {
        }
    }


    public class HtmlEntities
    {
        public HtmlEntities(
        )
        {
        }
    }


    public class HttpUtilityImpl
        : IHttpUtility
    {
        public HttpUtilityImpl(
        )
        {
        }
    }


    public interface IHttpUtility
    {
    }


    public interface IImageResizer
    {
    }


    public class ImageResizer
        : IImageResizer
    {
        public ImageResizer(
        )
        {
        }
    }


    public interface IObjectDescription
    {
    }

    public interface IPropertyAccessor
    {
    }


    public class PropertyAccessorBase
        : IPropertyAccessor
    {
    }


    public class PropertyAccessor : PropertyAccessorBase
    {
        public PropertyAccessor(
            PropertyInfo arg0
        ) : base()
        {
            field0 = arg0;
        }

        public PropertyAccessor(
            FieldInfo arg0
        ) : base()
        {
            field0 = arg0;
        }

        public PropertyAccessor(
            Type arg0,
            string arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public PropertyAccessor(
            object arg0,
            string arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly object field0;
        public readonly string field1;
    }


    public class ConstantPropertyAccessor : PropertyAccessorBase
    {
        public ConstantPropertyAccessor(
            object arg0,
            string arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly object field0;
        public readonly string field1;
    }

    public class UtcTimePropertyAccessor : PropertyAccessor
    {
        public UtcTimePropertyAccessor(
            PropertyInfo arg0
        ) : base(arg0)
        {
        }

        public UtcTimePropertyAccessor(
            FieldInfo arg0
        ) : base(arg0)
        {
        }

        public UtcTimePropertyAccessor(
            Type arg0,
            string arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class PropertyUtil
    {
    }


    public class Registry
    {
        public Registry(
        )
        {
        }
    }


    public class Rounding
    {
        public Rounding(
        )
        {
        }
    }


    public class Truncate
    {
        public Truncate(
        )
        {
        }
    }


    public class GetEntityStringDelegate
    {
        public GetEntityStringDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly object field0;
        public readonly IntPtr field1;
    }


    public class Util
    {
        public Util(
        )
        {
        }
    }


    public class CurrencyRoundOption
    {
    }


    public class ChunkType
    {
    }


    public class FieldParseStatus
    {
    }


    public class FieldType
    {
    }


    public class EventLogEntryType
    {
    }


    public class EventLogId
    {
    }


    public class PaddingDirection
    {
    }


    public class ColumnInfo
    {
        public ColumnInfo(
        )
        {
        }
    }


    public class Edition
    {
    }


    public class HashPathNotFoundException : Exception
    {
        public HashPathNotFoundException(
            string arg0
        ) : base(arg0)
        {
        }
    }


    public class SquareResizeMode
    {
    }


    public class GetObjectValue
    {
        public GetObjectValue(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly object field0;
        public readonly IntPtr field1;
    }


    public class CreateObjectForCacheDelegate
    {
        public CreateObjectForCacheDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly object field0;
        public readonly IntPtr field1;
    }


    public class EnumerableHandlerDelegate
    {
        public EnumerableHandlerDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly object field0;
        public readonly IntPtr field1;
    }
}