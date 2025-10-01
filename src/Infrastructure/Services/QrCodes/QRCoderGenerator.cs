using EazyMenu.Application.Features.QrCodes;
using QRCoder;
using System;

namespace EazyMenu.Infrastructure.Services.QrCodes;

public sealed class QRCoderGenerator : IQrCodeGenerator
{
    public byte[] GenerateQrCodePng(string content, int pixelsPerModule = 20)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        
        return qrCode.GetGraphic(pixelsPerModule);
    }

    public string GenerateQrCodeSvg(string content, int pixelsPerModule = 20)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new SvgQRCode(qrCodeData);
        
        return qrCode.GetGraphic(pixelsPerModule);
    }

    public string GenerateQrCodeBase64(string content, int pixelsPerModule = 20)
    {
        var pngBytes = GenerateQrCodePng(content, pixelsPerModule);
        return Convert.ToBase64String(pngBytes);
    }
}
