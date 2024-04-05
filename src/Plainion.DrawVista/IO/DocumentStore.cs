using Newtonsoft.Json;
using Plainion.DrawVista.UseCases;

namespace Plainion.DrawVista.IO;

public class DocumentStore(string rootFolder) : IDocumentStore
{
    private readonly object myLock = new object();

    private record MetaContent(List<string> Captions);

    public string RootFolder { get; private set; } = rootFolder;

    public event Action StoreFilesChanged;

    private string MetaFile(string pageName) => Path.Combine(RootFolder, pageName + ".svg.meta");

    private string ContentFile(string pageName) => Path.Combine(RootFolder, pageName + ".svg");

    protected virtual void OnStoreChanged()
    {
        StoreFilesChanged?.Invoke();
    }

    public IReadOnlyCollection<string> GetPageNames()
    {
        lock (myLock)
        {
            return Directory.GetFiles(RootFolder, "*.svg")
                .Select(Path.GetFileNameWithoutExtension)
                .ToList();
        }
    }

    public ProcessedDocument GetPage(string pageName)
    {
        lock (myLock)
        {
            var content = File.ReadAllText(ContentFile(pageName));
            dynamic meta = JsonConvert.DeserializeObject<MetaContent>(File.ReadAllText(MetaFile(pageName))); 
            return new ProcessedDocument(pageName, content, meta?.Captions);
        }
    }

    public void Save(ProcessedDocument document)
    {
        lock (myLock)
        {
            File.WriteAllText(ContentFile(document.Name), document.Content);
            var meta = new MetaContent(document.Captions?.ToList());
            File.WriteAllText(MetaFile(document.Name), JsonConvert.SerializeObject(meta));
        }
        if (!document.Name.Equals("index"))
        {
            OnStoreChanged();
        }
    }

    public void Clear()
    {
        lock (myLock)
        {
            foreach (var file in Directory.GetFiles(RootFolder))
            {
                File.Delete(file);
            }
        }
    }
}
