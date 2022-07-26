using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Agent;

public class ScreenshotService
{
    public byte[] GetImageBytes()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return Array.Empty<byte>();

        var image = WindowsScreenCapture.CaptureDesktop();

#pragma warning disable CA1416
        var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
#pragma warning restore CA1416
        var encParams = new EncoderParameters() { Param = new[] { new EncoderParameter(Encoder.Quality, 80L) } };

        using var stream = new MemoryStream();
        image.Save(stream, encoder, encParams);
        return stream.ToArray();
    }
}