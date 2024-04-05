using Plainion.DrawVista.UseCases;

namespace Plainion.DrawVista.Adapters;

public class DocumentStoreCachingDecorator(IDocumentStore impl) : IDocumentStore
{
    private readonly Dictionary<string, ProcessedDocument> myCache = impl
        .GetPageNames()
        .Select(impl.GetPage)
        .ToDictionary(x => x.Name);

    public event Action StoreFilesChanged;

    public string RootFolder
    {
        get
        {
            return impl.RootFolder;
        }
    }
    private void OnStoreChanged()
    {
        StoreFilesChanged?.Invoke();
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
        myCache[document.Name] = document;
        if (!document.Name.Equals("index"))
        {
            OnStoreChanged();
        }
    }
}
