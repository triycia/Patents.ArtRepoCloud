using System.Xml;

namespace Patents.ArtRepoCloud.Service.DataFetchers
{
    public class ImageXMetadata
    {
        internal const string OpsNamespacePrefix = "ops";
        internal const string OpsNamespaceUri = "http://ops.epo.org";
        internal const string EpoExchangeNamespaceUri = "http://www.epo.org/exchange";
        internal const string XsdNamespacePrefix = "xsd";
        internal const string XsdNamespaceUri = "http://www.w3.org/2001/XMLSchema";
        internal const string XsiNamespacePrefix = "xsi";
        internal const string XsiNamespaceUri = "http://www.w3.org/2001/XMLSchema-instance";
        internal const string LinkXPath = "/ops:world-patent-data/ops:document-inquiry/ops:inquiry-result/ops:document-instance[@desc='FullDocument']/@link";
        internal const string NumberOfPagesXPath = "/ops:world-patent-data/ops:document-inquiry/ops:inquiry-result/ops:document-instance[@desc='FullDocument']/@number-of-pages";
        internal const string DrawingStartPageXPath = "/ops:world-patent-data/ops:document-inquiry/ops:inquiry-result/ops:document-instance[@desc='FullDocument']/ops:document-section[@name='DRAWINGS']/@start-page";
        internal const string NumberOfDrawingPagesXPath = "/ops:world-patent-data/ops:document-inquiry/ops:inquiry-result/ops:document-instance[@desc='Drawing']/@number-of-pages";
        
        public string Link { get; }
        public int NumberOfPages { get; }
        public int DrawingStartPage { get; }
        public int NumberOfDrawingPages { get; }
        public int DrawingEndPage { get; }

        public ImageXMetadata(string xmlText)
        {
            var doc = new XmlDocument();

            doc.LoadXml(xmlText);

            var ns = new XmlNamespaceManager(doc.NameTable);

            ns.AddNamespace(XsdNamespacePrefix, XsdNamespaceUri);
            ns.AddNamespace(XsiNamespacePrefix, XsiNamespaceUri);
            ns.AddNamespace(OpsNamespacePrefix, OpsNamespaceUri);
            ns.AddNamespace(string.Empty, EpoExchangeNamespaceUri);

            Link = doc.SelectSingleNode(LinkXPath, ns)?.InnerText;
            NumberOfPages = int.Parse(doc.SelectSingleNode(NumberOfPagesXPath, ns)?.InnerText ?? "0");

            var drawingStartPageNode = doc.SelectSingleNode(DrawingStartPageXPath, ns);
            var numberOfDrawingPagesNode = doc.SelectSingleNode(NumberOfDrawingPagesXPath, ns);

            if (drawingStartPageNode == null || numberOfDrawingPagesNode == null)
            {
                DrawingStartPage = 0;
                DrawingEndPage = 0;
            }
            else
            {
                DrawingStartPage = int.Parse(drawingStartPageNode.InnerText);
                NumberOfDrawingPages = int.Parse(numberOfDrawingPagesNode.InnerText);
                DrawingEndPage = DrawingStartPage + NumberOfDrawingPages - 1;
            }
        }
    }
}