using Plainion.DrawVista.UseCases;
using System.Xml.Linq;

namespace Plainion.DrawVista.IO
{
    public class IndexObserver
    {
        private readonly IDocumentStore myDocumentStore;
        private XDocument myIndexPageXdoc = new();

        public IndexObserver(IDocumentStore documentStore)
        {
            myDocumentStore = documentStore;
            myDocumentStore.StoreFilesChanged += DocumentStore_StoreFilesChanged;
            try
            {
                var pages = myDocumentStore.GetPageNames();
                if (pages.Contains("index"))
                {
                    var indexPageDoc = myDocumentStore.GetPage("index");
                    myIndexPageXdoc = XDocument.Parse(indexPageDoc.Content);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("No existing index page in the store could be found.");
            }
        }

        private void DocumentStore_StoreFilesChanged()
        {
            UpdateIndexPage();
            SaveXDocument(myIndexPageXdoc);
        }

        private XDocument AddEdges()
        {
            var allPages = myDocumentStore.GetPageNames();

            for (int i = 0; i < allPages.Count; i++)
            {
                if (allPages.ElementAt(i) == "index")
                {
                    continue;
                }
                var sourcePageNode = myDocumentStore.GetPage(allPages.ElementAt(i));
                var pageName = sourcePageNode.GetPageName();

                var sourceSvgxElement = myIndexPageXdoc.Descendants(SvgExtensions.Svgns + "image")
                    .Where(e => (string)e.Attribute("pageName") == pageName)
                    .FirstOrDefault();

                var sourceCenter = SvgExtensions.GetCenterOfImageNode(sourceSvgxElement);
                var refrences = GetReferencePages(pageName);

                for (int j = 0; j < refrences.Count; j++)
                {
                    var referenceCenter = SvgExtensions.GetCenterOfImageNodeByName(myIndexPageXdoc, refrences.ElementAt(j));
                    var arrow = SvgExtensions.GetArrow(sourceCenter, referenceCenter);

                    foreach (var element in arrow)
                    {
                        myIndexPageXdoc.Root.Add(element);
                    }
                }

                List<string> GetReferencePages(string pageNameToTest)
                {
                    List<string> result = new List<string>();
                    foreach (var page in allPages)
                    {
                        XDocument xDoc = XDocument.Parse(myDocumentStore.GetPage(page).Content);
                        var linkedPages = xDoc.Descendants(ConstantParams.LinksInfoTag).Attributes("label").Select(x => x.Value);
                        if (linkedPages.Contains(pageNameToTest))
                        {
                            result.Add(page);
                        }
                    }

                    return result;
                }
            }

            return myIndexPageXdoc;
        }

        private XDocument UpdateIndexPage()
        {
            var existingPagesNames = myDocumentStore.GetPageNames();
            var svg = SvgExtensions.getSvgHeaderElement();

            int yOffset = 0;
            int nodeWidth = 100;

            foreach (var pageName in existingPagesNames)
            {
                if (pageName == "index")
                    continue;
                try
                {
                    var pngFilePath = Path.Combine(myDocumentStore.RootFolder, $"{pageName}.png");
                    XElement.Parse(myDocumentStore.GetPage(pageName).Content).SVG2PNG(pngFilePath);
                    string base64Data = ImageHelper.ConvertPicToBase64(pngFilePath);
                    var dimensions = ImageHelper.GetImageDimensions(pngFilePath);
                    var newHeight = ImageHelper.CalculateHeightKeepingScalefactor(nodeWidth, dimensions);

                    svg.Add(SvgExtensions.GetPngXElement(base64Data, pageName, yOffset, nodeWidth, newHeight));

                    yOffset += newHeight + 20;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to add page {pageName} to index page. Error: {e}");
                }
            }

            myIndexPageXdoc = new XDocument(svg);

            try
            {
                myIndexPageXdoc = AddEdges();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not add links to the index page. Error: {e}");
            }

            return myIndexPageXdoc;
        }

        private void SaveXDocument(XDocument xDoc)
        {
            ProcessedDocument indexProcessedDoc = new ProcessedDocument("index", xDoc.ToString(), null);
            myDocumentStore.Save(indexProcessedDoc);
        }
    }
}
