namespace Pdf
{
    public class DocumentAreaComponent : DocumentBaseComponent
    {
        public DocumentAreaComponent(
            string arg0
        ) : base(arg0)
        {
        }
    }


    public class DocumentBaseComponent
    {
        public DocumentBaseComponent(
            string arg0,
            string arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly string field1;

        protected DocumentBaseComponent(string arg0)
        {
        }

        protected DocumentBaseComponent(int arg0, string s)
        {
        }
    }


    public class DocumentFieldComponent : DocumentBaseComponent
    {
        public DocumentFieldComponent(
            string arg0
        ) : base(arg0)
        {
        }
    }


    public class DocumentImageComponent : DocumentBaseComponent
    {
        public DocumentImageComponent(
            string arg0
        ) : base(arg0)
        {
        }
    }


    public class DocumentLineComponent : DocumentBaseComponent
    {
        public DocumentLineComponent(
            string arg0
        ) : base(arg0)
        {
        }
    }


    public class DocumentRectComponent : DocumentBaseComponent
    {
        public DocumentRectComponent(
            string arg0
        ) : base(arg0)
        {
        }
    }


    public class DocumentTableCellComponent : DocumentBaseComponent
    {
        public DocumentTableCellComponent(
            int arg0,
            string arg1,
            bool arg2
        ) : base(arg0, arg1)
        {
            field2 = arg2;
        }

        public readonly bool field2;
    }


    public class DocumentTableComponent : DocumentBaseComponent
    {
        public DocumentTableComponent(
            string arg0,
            int arg1,
            string arg2,
            int arg3,
            int? arg4,
            int? arg5
        ) : base(arg0, arg2)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly string field2;
        public readonly int field3;
        public readonly int? field4;
        public readonly int? field5;
    }


    public class DocumentTableRowComponent : DocumentBaseComponent
    {
        public DocumentTableRowComponent(
            string arg0
        ) : base(arg0)
        {
        }
    }


    public interface IDocumentArea
    {
    }


    public interface IImageDataObject
    {
    }


    public class ImageDataObject
        : IImageDataObject
    {
        public ImageDataObject(
            Doc arg0
        )
        {
            field0 = arg0;
        }

        public readonly Doc field0;
    }

    public class Doc
    {
    }

    public interface IInvoiceBreakdownDefinition
        : IPdfXmlDefinitionBase
    {
    }


    public class InvoiceBreakdownDefinition : PdfXmlDefinitionBase
        , IInvoiceBreakdownDefinition
    {
        public InvoiceBreakdownDefinition(
        )
        {
        }
    }


    public interface IInvoiceDefinition
    {
    }


    public class InvoiceDefinition
        : IInvoiceDefinition
    {
        public InvoiceDefinition(
        )
        {
        }
    }


    public interface IPdfDocument
    {
    }


    public class PdfDocument
        : IPdfDocument
    {
        public PdfDocument(
        )
        {
        }
    }


    public interface IPdfDocumentContainerService
    {
    }


    public class PdfDocumentContainerService
        : IPdfDocumentContainerService
    {
        public PdfDocumentContainerService(
        )
        {
        }
    }


    public interface IPdfDocumentService
    {
    }


    public class PdfDocumentService
        : IPdfDocumentService
    {
        public PdfDocumentService(
            IPdfDocumentContainerService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IPdfDocumentContainerService field0;
    }


    public interface IPdfXmlDefinitionBase
    {
    }


    public interface IProposalDefinition
        : IPdfXmlDefinitionBase
    {
    }


    public class ProposalDefinition : PdfXmlDefinitionBase
        , IProposalDefinition
    {
        public ProposalDefinition(
        )
        {
        }
    }


    public interface ITravelReimbursementBreakdownDefinition
        : IPdfXmlDefinitionBase
    {
    }


    public class TravelReimbursementBreakdownDefinition : PdfXmlDefinitionBase
        , ITravelReimbursementBreakdownDefinition
    {
        public TravelReimbursementBreakdownDefinition(
        )
        {
        }
    }


    public interface ITravelReimbursementDefinition
        : IPdfXmlDefinitionBase
    {
    }


    public class TravelReimbursementDefinition : PdfXmlDefinitionBase
        , ITravelReimbursementDefinition
    {
        public TravelReimbursementDefinition(
        )
        {
        }
    }


    public class PdfTable
    {
        public PdfTable(
        )
        {
        }
    }


    public class PdfXmlDefinitionBase
        : IPdfXmlDefinitionBase
    {
    }


    public partial class RenderInfo
    {
        public RenderInfo(
            int arg0,
            double arg1,
            double arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly int field0;
        public readonly double field1;
        public readonly double field2;
    }


    public class TestDefinition
    {
        public TestDefinition(
        )
        {
        }
    }


    public class CoordinatePair
    {
        public CoordinatePair(
        )
        {
        }
    }


    public class ColorType
    {
    }


    public class BackgroundColor
    {
        public BackgroundColor(
        )
        {
        }
    }


    public class TruncatedCell
    {
        public TruncatedCell(
        )
        {
        }
    }


    public class DocumentLocation
    {
    }


    public class DocumentInfo
    {
        public DocumentInfo(
        )
        {
        }
    }


    public class TextAlignment
    {
    }


    public partial class RenderInfo
    {
        public RenderInfo(
        )
        {
        }
    }
}