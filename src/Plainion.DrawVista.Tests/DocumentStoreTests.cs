using Plainion.DrawVista.Adapters;
using Plainion.DrawVista.IO;
using Plainion.DrawVista.UseCases;

namespace Plainion.DrawVista.Tests;

[TestFixture]
public class DocumentStoreTests
{
    private readonly string myRootFolder = Path.Combine(Path.GetTempPath(), "DrawVista.Store");

    [SetUp]
    public void SetUp()
    {
        Directory.CreateDirectory(myRootFolder);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(myRootFolder))
        {
            Directory.Delete(myRootFolder, true);
        }
    }

    [Test]
    public void StoreAndLoad()
    {
        var store = new DocumentStore(myRootFolder);

        store.Save(new ProcessedDocument("Page-1", "Some dummy content", ["caption-1", "caption-2"]));
        var document = store.GetPage("Page-1");

        Assert.That(document.Captions, Is.EquivalentTo(new[] { "caption-1", "caption-2" }));
    }

    [Test]
    public void SaveTriggersOnStoreCHangedEvent()
    {
        var store = new DocumentStore(myRootFolder);
        var wasCalled = false;
        store.StoreFilesChanged += delegate { wasCalled = true; };

        store.Save(new ProcessedDocument("Page-1", "Some dummy content", ["caption-1", "caption-2"]));

        Assert.IsTrue(wasCalled);
    }

    [Test]
    public void SaveIndexDoesNotTriggerOnStoreCHangedEvent()
    {
        var store = new DocumentStore(myRootFolder);
        var wasCalled = false;
        store.StoreFilesChanged += delegate { wasCalled = true; };

        store.Save(new ProcessedDocument("index", "Some dummy content", ["caption-1", "caption-2"]));

        Assert.IsFalse(wasCalled);
    }

    [Test]
    public void ClearDoesNotTriggersOnStoreCHangedEvent()
    {
        var store = new DocumentStore(myRootFolder);
        var wasCalled = false;
        store.StoreFilesChanged += delegate { wasCalled = true; };

        store.Clear();

        Assert.IsFalse(wasCalled);
    }
}
