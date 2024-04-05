using System.Diagnostics;
using System.Xml.Linq;

namespace Plainion.DrawVista.IO
{
    

    internal class DrawIOAppExtension(DrawIOModel Model) : DrawIOApp(Model), IDrawIOApp
    {
        public static XName LinksInfoTag = "HyperLinks";
        public static XName PageInfoTag = "PageInfo";

        public new void ExtractSvg(int pageIndex, string svgFile)
        {
            SaveModelOnDemand();
            Process.Start(Executable, $"-x {myFile} -o {svgFile} -p {pageIndex}")
                .WaitForExit();

            AppendPageLinkingInfoToSvg(pageIndex, myFile, svgFile);
        }

        private void AppendPageLinkingInfoToSvg(int pageIndex, string drawIOFilePath, string svgFileName)
        {
            XDocument svgDocument = XDocument.Load(svgFileName);
            XDocument documentToInspect = XDocument.Load(drawIOFilePath);
            var diagram = documentToInspect.Descendants("diagram")
                .Where(d => d.Attribute("name") != null);

            var pageInfo = GetPageInfo(pageIndex, diagram);
            svgDocument.Root.Add((object)pageInfo);

            var linkInfos = GetLinkInfos(pageInfo.Attribute("name")?.Value, diagram);
            foreach (var linkInfo in linkInfos)
            {
                svgDocument.Root.Add((object)linkInfo);
            }
            svgDocument.Save(svgFileName);
        }

        private XElement GetPageInfo(int pageIndex, IEnumerable<XElement> diagram)
        {
            List<XElement> clonedDiagram = diagram.Select(element => new XElement(element)).ToList();
            clonedDiagram.Descendants("mxGraphModel").Remove();
            XElement pageInfo = new XElement(PageInfoTag, clonedDiagram.ElementAt(pageIndex).Attributes(), clonedDiagram.ElementAt(pageIndex).Elements());

            return pageInfo;
        }

        private List<XElement> GetLinkInfos(string pageName, IEnumerable<XElement> diagram)
        {
            // Query for the UserObject element with the specified attributes
            var linkObjects = diagram.Descendants("UserObject")
                .Where(uo => uo.Attribute("label").Value != pageName
                             && uo.Attribute("link") != null
                             && uo.Attribute("link").Value.StartsWith("data:page/id,"));
            IEnumerable<XElement> clonedLinkObjects = linkObjects.Select(element => new XElement(element)).ToList();
            clonedLinkObjects.Descendants("mxCell").Remove();

            return linkObjects.Select(o => new XElement(LinksInfoTag, o.Attributes(), o.Elements())).ToList();
        }
    }
}
