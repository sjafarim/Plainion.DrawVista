using System.Xml.Linq;
using Plainion.DrawVista.IO;

namespace Plainion.DrawVista.UseCases;

public record RawDocument(string Name, string Content);

public record Caption(XElement Element, string DisplayText);

public record ProcessedDocument(string Name, string Content, IReadOnlyCollection<string> Captions)
{
    public string GetPageName()
    {
        try
        {
            XDocument sourcePageNodexDoc = XDocument.Parse(Content);
            var pageName = sourcePageNodexDoc.Descendants(ConstantParams.PageInfoTag).FirstOrDefault().Attribute("name")?.Value;
            return pageName;
        }
        catch (Exception e)
        {
            return Name;
        }
    }

    public string GetPageID()
    {
        XDocument sourcePageNodexDoc = XDocument.Parse(Content);
        var pageID = sourcePageNodexDoc.Descendants(ConstantParams.PageInfoTag).FirstOrDefault().Attribute("id")?.Value;
        return pageID;
    }
}
