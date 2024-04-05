using SkiaSharp;
using Svg.Skia;
using System.Xml.Linq;

namespace Plainion.DrawVista.UseCases;

static class SvgExtensions
{
    public static XNamespace Svgns = "http://www.w3.org/2000/svg";
    public static XNamespace Xlink = "http://www.w3.org/1999/xlink";

    public static bool EqualsTagName(this XElement self, string name) =>
        self.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase);

    public static bool IsMostInnerDiv(this XElement self)=>
        self.EqualsTagName("div") && !self.Elements().Any(x => x.EqualsTagName("div"));

    public static XElement getSvgHeaderElement()
    {
        var svg = new XElement(Svgns + "svg",
            new XAttribute("xmlns", Svgns),
            new XAttribute("version", "1.1"),
            new XAttribute("width", "100%"),
            new XAttribute("height", "800"));
        return svg;
    }

    public static XElement GetPngXElement(string base64Data, string pageName, int yOffset, int nodeWidth, int newHeight)
    {
        return new XElement(Svgns + "image",
            new XAttribute(XNamespace.Xmlns + "xlink", Xlink),
            new XAttribute(Xlink + "href", $"data:image/png;base64,{base64Data}"),
            new XAttribute("pageName", pageName),
            new XAttribute("class", "clickable"),
            new XAttribute("x", "0"), 
            new XAttribute("y", yOffset),
            new XAttribute("width", nodeWidth), 
            new XAttribute("onclick", $"window.hook.navigate('Page-1'),"),
            new XAttribute("height", newHeight)); 
    }

    public static void SVG2PNG(XElement svg, string fileName)
    {
        string svgText = svg.ToString();

        var SKsvg = new SKSvg();
        SKsvg.FromSvg(svgText);

        int borderSize = 10;
        SKColor borderColor = SKColors.Blue;

        int widthWithBorder = (int)SKsvg.Picture.CullRect.Width + borderSize * 2;
        int heightWithBorder = (int)SKsvg.Picture.CullRect.Height + borderSize * 2;

        using (var bitmap = new SKBitmap(widthWithBorder, heightWithBorder))
        {
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);
                var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = borderColor,
                    StrokeWidth = borderSize
                };
                canvas.DrawRect(0, 0, widthWithBorder, heightWithBorder, paint);
                canvas.Translate(borderSize / 2, borderSize / 2);
                canvas.DrawPicture(SKsvg.Picture);

                using (var image = SKImage.FromBitmap(bitmap))
                {
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                    {
                        using (var stream = File.OpenWrite(fileName))
                        {
                            data.SaveTo(stream);
                        }
                    }
                }
            }
        }
    }

    public static Tuple<int, int> GetCenterOfImageNodeByName(XDocument indexPagexDoc, string pageName)
    {
        var referenceSVGXElement = indexPagexDoc.Descendants(Svgns + "image")
            .Where(x => x.Attribute("pageName").Value.Equals(pageName))
            .FirstOrDefault();
        return GetCenterOfImageNode(referenceSVGXElement);
    }

    public static Tuple<int, int> GetCenterOfImageNode(XElement referenceSVGXElement)
    {
        var referenceX = int.Parse(referenceSVGXElement.Attribute("x").Value);
        var referenceY = int.Parse(referenceSVGXElement.Attribute("y").Value);
        var referenceWidth = int.Parse(referenceSVGXElement.Attribute("width").Value);
        var referenceHeight = int.Parse(referenceSVGXElement.Attribute("height").Value);
        var referenceCenter = new Tuple<int, int>(referenceX + referenceWidth / 2, referenceY + referenceHeight / 2);
        return referenceCenter;
    }

    public static List<XElement> GetArrow(Tuple<int, int> sourceCenter, Tuple<int, int> referenceCenter)
    {
        List<XElement> arrow = new();
        // Create the marker for the arrowhead
        XElement marker = new XElement("marker",
            new XAttribute("id", "arrowhead"),
            new XAttribute("markerWidth", "5"),
            new XAttribute("markerHeight", "5"),
            new XAttribute("refX", "0"),
            new XAttribute("refY", "2.5"),
            new XAttribute("orient", "auto-start-reverse"),
            new XElement("polygon", new XAttribute("points", "0,0 5,2.5 0,5"))
        );

        // Create the line that uses the arrowhead marker
        var tail = new XElement("line",
            new XAttribute("x1", sourceCenter.Item1),
            new XAttribute("y1", sourceCenter.Item2),
            new XAttribute("x2", referenceCenter.Item1),
            new XAttribute("y2", referenceCenter.Item2),
            new XAttribute("stroke", "black"),
            new XAttribute("stroke-width", "1"),
            new XAttribute("marker-end", "url(#arrowhead)")
        );

        arrow.Add(marker);
        arrow.Add(tail);

        return arrow;
    }
}
