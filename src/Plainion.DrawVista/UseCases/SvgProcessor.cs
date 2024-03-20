using System.Xml.Linq;

namespace Plainion.DrawVista.UseCases;

public class SvgProcessor(ISvgCaptionParser parser, ISvgHyperlinkFormatter formatter, IDocumentStore store)
{
    private readonly ISvgCaptionParser myParser = parser;
    private readonly ISvgHyperlinkFormatter myFormatter = formatter;
    private readonly IDocumentStore myStore = store;

    /// <summary>
    /// Processes existing and newly uploaded documents.
    /// </summary>
    public void Process(IReadOnlyCollection<SvgDocument> documents)
    {
        var existingFiles = myStore.GetPageNames()
            .Where(x => !documents.Any(y => y.Name.Equals(x, StringComparison.OrdinalIgnoreCase)))
            .Select(x => SvgDocument.Create(myStore.GetPage(x)))
            .ToList();

        var knownPageNames = documents.Select(x => x.Name)
            .Concat(existingFiles.Select(x => x.Name))
            .ToList();

        foreach (var doc in documents.Concat(existingFiles))
        {
            AddLinks(knownPageNames, doc);
            ApplyStyleToExistingLinks(doc);
            myStore.Save(doc);
        }
    }


    private void AddLinks(IReadOnlyCollection<string> pages, SvgDocument document)
    {
        bool IsPageReference(string name) =>
           pages.Any(p => p.Equals(name, StringComparison.OrdinalIgnoreCase));

        var elementsReferencingPages = myParser.Parse(document.Content)
            .Where(x => IsPageReference(x.DisplayText))
            // skip self-references
            .Where(x => !x.DisplayText.Equals(document.Name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var caption in elementsReferencingPages)
        {
            Console.WriteLine($"Creating link for: {caption.DisplayText}");

            var onClickAttr = caption.Element.Attribute("onclick");
            if (onClickAttr == null)
            {
                onClickAttr = new XAttribute("onclick", string.Empty);
                caption.Element.Add(onClickAttr);
            }
            onClickAttr.Value = $"window.hook.navigate('{caption.DisplayText}')";

            myFormatter.ApplyStyle(caption.Element, isExternal:false);
        }

        document.Content.Attribute("width").Value = "100%";
    }

    // In DrawIO external links can be provided but those are neither in draw.io
    // nor in SVG visualized as links (e.g. blue and underlined) - so let's apply some style
    private void ApplyStyleToExistingLinks(SvgDocument doc)
    {
        static bool EqualsTagName(XElement element, string name) =>
            element.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase);
        
        var existingLinks = doc.Content
            .Descendants()
            .Where(x => EqualsTagName(x, "a"))
            .SelectMany(x => x.Descendants()
                .Where(x => EqualsTagName(x, "div") && !x.Elements().Any(x => EqualsTagName(x, "div"))))
           .ToList();

        foreach (var link in existingLinks)
        {
            myFormatter.ApplyStyle(link, isExternal:true);
        }
    }
}