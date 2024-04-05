using System;
using System.Xml.Linq;
using static Plainion.DrawVista.Adapters.DocumentStoreCachingDecorator;

namespace Plainion.DrawVista.UseCases;

public interface IDocumentStore
{
    IReadOnlyCollection<string> GetPageNames();
    ProcessedDocument GetPage(string pageName);
    void Save(ProcessedDocument document);
    void Clear();

    delegate void Notify();
    event Notify StoreFilesChanged;

    String RootFolder { get; }
}
