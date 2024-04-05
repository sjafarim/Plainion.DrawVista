using SkiaSharp;

namespace Plainion.DrawVista.UseCases
{
    public static class ImageHelper
    {
        public static string ConvertPicToBase64(string filePath)
        {
            using (var imageStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[imageStream.Length];
                imageStream.Read(bytes, 0, bytes.Length);
                return Convert.ToBase64String(bytes);
            }
        }

        public static int CalculateHeightKeepingScalefactor(int nodeWidth, (int Width, int Height) dimensions)
        {
            float scaleFactor = (float)nodeWidth / dimensions.Width;
            int newHeight = (int)(dimensions.Height * scaleFactor);
            return newHeight;
        }

        public static(int Width, int Height) GetImageDimensions(string filePath)
        {
            using (var image = SKBitmap.Decode(filePath))
            {
                return (image.Width, image.Height);
            }
        }


    }
}
