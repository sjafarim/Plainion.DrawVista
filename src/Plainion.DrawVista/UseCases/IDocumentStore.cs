namespace Plainion.DrawVista.UseCases;

public interface IDocumentStore
{
    /// <summary>
    /// The root folder of the Document store to store files into on disk.
    /// </summary>
    string RootFolder { get; }

    /// <summary>
    /// Get the name of all pages existing in document store.
    /// </summary>
    /// <returns>collection of page available names</returns>
    IReadOnlyCollection<string> GetPageNames();

    /// <summary>
    /// Get a page by page name from the existing pages in the document store.
    /// </summary>
    /// <param name="pageName">Name of the page to be returned</param>
    /// <returns>Corresponding page matching the page name</returns>
    ProcessedDocument GetPage(string pageName);

    /// <summary>
    /// Save a document in document store.
    /// </summary>
    /// <param name="document">Document to be saved</param>
    void Save(ProcessedDocument document);

    /// <summary>
    /// Clear the document store. All files in document store will be wiped. 
    /// </summary>
    void Clear();

    /// <summary>
    /// Event that is triggered when a new save operation in document store is happened.
    /// </summary>
    event Action StoreFilesChanged;
}
