using Plainion.DrawVista.IO;
using Plainion.DrawVista.UseCases;
using SkiaSharp;
using Svg.Skia;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace Plainion.DrawVista.Adapters;

public class DocumentStoreCachingDecorator(IDocumentStore impl) : IDocumentStore
{
    private readonly Dictionary<string, ProcessedDocument> myCache = impl
        .GetPageNames()
        .Select(impl.GetPage)
        .ToDictionary(x => x.Name);

    protected virtual void OnStoreChanged()
    {
        StoreFilesChanged?.Invoke();
    }

    public event IDocumentStore.Notify StoreFilesChanged;

    public string RootFolder
    {
        get
        {
            return impl.RootFolder;
        }
    }

    public void Clear()
    {
        myCache.Clear();
        impl.Clear();
    }

    public ProcessedDocument GetPage(string pageName) =>
        myCache[pageName];

    public IReadOnlyCollection<string> GetPageNames() =>
        myCache.Keys.ToList();

    public void Save(ProcessedDocument document)
    {
        impl.Save(document);
        CachedDocument(document);
        if (!document.Name.Equals("index"))
        {
            OnStoreChanged();
        }
    }

    private void CachedDocument(ProcessedDocument document)
    {
        myCache[document.Name] = document;
    }

    
}
