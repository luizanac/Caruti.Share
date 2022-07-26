using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Agent;

public class ScreenshotService
{
    // public async Task<byte[]> GetImageBytes(string fileName)
    // {
    //     var argument = $"ffmpeg -f gdigrab -i desktop -an -frames:v 1 -q:v 1 {fileName} -y";
    //
    //     var cmd = new Process
    //     {
    //         StartInfo = new ProcessStartInfo
    //         {
    //             FileName = "powershell",
    //             Arguments = argument,
    //             RedirectStandardInput = false,
    //             RedirectStandardOutput = false,
    //             CreateNoWindow = false,
    //             UseShellExecute = false
    //         }
    //     };
    //     cmd.Start();
    //     await cmd.WaitForExitAsync();
    //     await using var fileStream = File.OpenRead(fileName);
    //     using var memoryStream = new MemoryStream();
    //     await fileStream.CopyToAsync(memoryStream);
    //     return memoryStream.ToArray();
    // }    

    public byte[] GetImageBytes(string fileName)
    {
        const int width = 2560;
        const int height = 1080;

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