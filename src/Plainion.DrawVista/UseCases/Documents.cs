
using System.Xml.Linq;
using Plainion.DrawVista.IO;

namespace Plainion.DrawVista.UseCases;

public record RawDocument(string Name, string Content);

public record ProcessedDocument(string Name, string Content, IReadOnlyCollection<string> Captions)
{
    public string getPageName()
    {
        try
        {
            XDocument sourcePageNodexDoc = XDocument.Parse(Content);
            var pageName = sourcePageNodexDoc.Descendants(DrawIOAppExtension.PageInfoTag).FirstOrDefault().Attribute("name")?.Value;
            return pageName;
        }
        catch (Exception e)
        {
            return Name;
        }
    }

    public string getPageID()
    {
        XDocument sourcePageNodexDoc = XDocument.Parse(Content);
        var pageID = sourcePageNodexDoc.Descendants(DrawIOAppExtension.PageInfoTag).FirstOrDefault().Attribute("id")?.Value;
        return pageID;
    }
}
