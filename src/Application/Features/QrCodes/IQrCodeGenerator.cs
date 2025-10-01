namespace EazyMenu.Application.Features.QrCodes;

/// <summary>
/// Interface برای سرویس تولید QR Code
/// </summary>
public interface IQrCodeGenerator
{
    /// <summary>
    /// تولید QR Code به صورت byte array (PNG)
    /// </summary>
    byte[] GenerateQrCodePng(string content, int pixelsPerModule = 20);

    /// <summary>
    /// تولید QR Code به صورت SVG string
    /// </summary>
    string GenerateQrCodeSvg(string content, int pixelsPerModule = 20);

    /// <summary>
    /// تولید QR Code به صورت Base64 string (برای نمایش در HTML)
    /// </summary>
    string GenerateQrCodeBase64(string content, int pixelsPerModule = 20);
}
