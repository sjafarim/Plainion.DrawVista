using Plainion.DrawVista.UseCases;
using System.Xml.Linq;

namespace Plainion.DrawVista.IO
{
    public class IndexObserver
    {
        private readonly IDocumentStore myDocumentStore;

        public IndexObserver(IDocumentStore documentStore)
        {
            myDocumentStore = documentStore;
            myDocumentStore.StoreFilesChanged += DocumentStore_StoreFilesChanged;
        }

        private void DocumentStore_StoreFilesChanged()
        {
            UpdateIndexPage();
        }

        internal void UpdateIndexPage()
        {
            var existingPagesNames = myDocumentStore.GetPageNames();
            var svg = SvgExtensions.getSvgHeaderElement();

            int yOffset = 0; 
            int nodeWidth = 100;

            foreach (var pageName in existingPagesNames)
            {
                if (pageName == "index")
                    continue;

                var pngFilePath = Path.Combine(myDocumentStore.RootFolder, $"{pageName}.png");
                SvgExtensions.SVG2PNG(XElement.Parse(myDocumentStore.GetPage(pageName).Content), pngFilePath);

                string base64Data = ImageHelper.ConvertPicToBase64(pngFilePath);
                var dimensions = ImageHelper.GetImageDimensions(pngFilePath);
                var newHeight = ImageHelper.CalculateHeightKeepingScalefactor(nodeWidth, dimensions);

                svg.Add(SvgExtensions.GetPngXElement(base64Data, pageName, yOffset, nodeWidth, newHeight));

                yOffset += newHeight + 20;
            }

            SaveXelement(svg);
            AddEdges();
        }

        private void AddEdges()
        {
            var allPages = myDocumentStore.GetPageNames();
            var indexPageDoc = myDocumentStore.GetPage("index");
            XDocument indexPagexDoc = XDocument.Parse(indexPageDoc.Content);

            for (int i = 0; i < allPages.Count; i++)
            {
                if (allPages.ElementAt(i) == "index")
                {
                    continue;
                }
                var sourcePageNode = myDocumentStore.GetPage(allPages.ElementAt(i));
                var pageName = sourcePageNode.getPageName();

                var sourceSVGXElement = indexPagexDoc.Descendants(SvgExtensions.Svgns + "image")
                    .Where(e => (string)e.Attribute("pageName") == pageName)
                    .FirstOrDefault();

                var sourceCenter = SvgExtensions.GetCenterOfImageNode(sourceSVGXElement);
                var refrences = GetReferencePages(pageName);

                for (int j = 0; j < refrences.Count; j++)
                {
                    var referenceCenter = SvgExtensions.GetCenterOfImageNodeByName(indexPagexDoc, refrences.ElementAt(j));
                    var arrow = SvgExtensions.GetArrow(sourceCenter, referenceCenter);

                    foreach (var element in arrow)
                    {
                        indexPagexDoc.Root.Add(element);
                    }
                }

                SaveXDocument(indexPagexDoc);

                List<string> GetReferencePages(string pageNameToTest)
                {
                    List<string> result = new List<string>();
                    foreach (var page in allPages)
                    {
                        XDocument xDoc = XDocument.Parse(myDocumentStore.GetPage(page).Content);
                        var LinkedPages = xDoc.Descendants(DrawIOAppExtension.LinksInfoTag).Attributes("label").Select(x => x.Value);
                        if (LinkedPages.Contains(pageNameToTest))
                        {
                            result.Add(page);
                        }
                    }

                    return result;
                }
            }
        }

        private void SaveXelement(XElement XElement)
        {
            ProcessedDocument indexProcessedDoc = new ProcessedDocument("index", XElement.ToString(), null);
            myDocumentStore.Save(indexProcessedDoc);
        }

        private void SaveXDocument(XDocument xDoc)
        {
            ProcessedDocument indexProcessedDoc = new ProcessedDocument("index", xDoc.ToString(), null);
            myDocumentStore.Save(indexProcessedDoc);
        }
    }
}
