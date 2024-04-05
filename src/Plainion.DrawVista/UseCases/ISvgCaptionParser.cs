using System.Xml.Linq;

namespace Plainion.DrawVista.UseCases;

public interface ISvgCaptionParser
{
    IReadOnlyCollection<Caption> Parse(XElement document);
}

