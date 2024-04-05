using Plainion.DrawVista.UseCases;
using System.Diagnostics;
using System.Xml.Linq;

namespace Plainion.DrawVista.IO
{
    public class DrawIOAppExtension(DrawIOModel Model) : DrawIOApp(Model), IDrawIOApp
    {
        private XDocument AppendPageLinkingInfoToSvg(int pageIndex, string drawIOFilePath, string svgFileName)
        {
            XDocument svgDocument = XDocument.Load(svgFileName);
            XDocument documentToInspect = XDocument.Load(drawIOFilePath);
            svgDocument = AppendPageLinkingInfoToSvg(pageIndex, documentToInspect, svgDocument);
            svgDocument.Save(svgFileName);
            return svgDocument;
        }

        private XDocument AppendPageLinkingInfoToSvg(int pageIndex, XDocument drawIoDocumentToInspect, XDocument svgDocument)
        {
            var mySvgDocument = new XDocument(svgDocument);
            var diagram = drawIoDocumentToInspect.Descendants("diagram")
                .Where(d => d.Attribute("name") != null);

            var pageInfo = GetPageInfo(pageIndex, diagram);
            mySvgDocument.Root.Add((object)pageInfo);

            var linkInfos = GetLinkInfos(pageInfo.Attribute("name")?.Value, diagram);
            foreach (var linkInfo in linkInfos)
            {
                mySvgDocument.Root.Add((object)linkInfo);
            }

            return mySvgDocument;
        }

        private XElement GetPageInfo(int pageIndex, IEnumerable<XElement> diagram)
        {
            List<XElement> clonedDiagram = diagram.Select(element => new XElement(element)).ToList();
            clonedDiagram.Descendants("mxGraphModel").Remove();
            XElement pageInfo = new XElement(ConstantParams.PageInfoTag, clonedDiagram.ElementAt(pageIndex).Attributes(), clonedDiagram.ElementAt(pageIndex).Elements());

            return pageInfo;
        }

        private List<XElement> GetLinkInfos(string pageName, IEnumerable<XElement> diagram)
        {
            var linkObjects = diagram.Descendants("UserObject")
                .Where(uo => uo.Attribute("label").Value != pageName
                             && uo.Attribute("link") != null
                             && uo.Attribute("link").Value.StartsWith("data:page/id,"));
            IEnumerable<XElement> clonedLinkObjects = linkObjects.Select(element => new XElement(element)).ToList();
            clonedLinkObjects.Descendants("mxCell").Remove();

            return linkObjects.Select(o => new XElement(ConstantParams.LinksInfoTag, o.Attributes(), o.Elements())).ToList();
        }

        public override XDocument ExtractSvg(int pageIndex, string svgFile)
        {
            SaveModelOnDemand();
            Process.Start(Executable, $"-x {myFile} -o {svgFile} -p {pageIndex}")
                .WaitForExit();

            return AppendPageLinkingInfoToSvg(pageIndex, myFile, svgFile);
        }
    }
}
