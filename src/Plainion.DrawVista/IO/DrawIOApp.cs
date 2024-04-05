using System.Diagnostics;
using System.Xml.Linq;

namespace Plainion.DrawVista.IO;

public class DrawIOApp(DrawIOModel Model) : IDisposable, IDrawIOApp
{
    protected static readonly string Executable = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
         "draw.io", "draw.io.exe");

    protected string myFile;

    public virtual XDocument ExtractSvg(int pageIndex, string svgFile)
    {
        SaveModelOnDemand();

        // Console.WriteLine($"draw.io.exe -x {myFile} -o {svgFile} -p {pageIndex}");

        Process.Start(Executable, $"-x {myFile} -o {svgFile} -p {pageIndex}")
            .WaitForExit();

        XDocument svgDocument = XDocument.Load(svgFile);

        return svgDocument;
    }

    public void SaveModelOnDemand()
    {
        if (myFile != null)
        {
            return;
        }

        // extension ".drawio" is important so that draw.io.exe detects file contents properly
        myFile = Path.GetTempFileName() + ".drawio";

        Model.WriteTo(myFile);
    }

    public void Dispose()
    {
        if (File.Exists(myFile))
        {
            File.Delete(myFile);
        }
    }
}