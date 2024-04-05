using System.Xml.Linq;

namespace Plainion.DrawVista.IO
{
    public interface IDrawIOApp : IDisposable
    {
        /// <summary>
        /// Extract a page from given DrawIO file which was set with class constructor and saves it as SVG file.
        /// </summary>
        /// <param name="pageIndex">Page index number to be extracted</param>
        /// <param name="svgFile">Path of the SVG to save the result file into</param>
        /// <returns>XDocument that is parsed version of resulting svg file</returns>
        XDocument ExtractSvg(int pageIndex, string svgFile);
    }
}
