using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OtpNet;
using QRCoder;

namespace OliverBooth.Pages.Blog.Admin;

public class Login : PageModel
{
    public string QrCode { get; set; }

    public string Secret { get; set; }

    public IActionResult OnGet()
    {
        if (Request.Cookies.ContainsKey("sid"))
        {
            return RedirectToPage("index");
        }

        Secret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));

        var uri = $"otpauth://totp/oliverbooth.dev?secret={Secret}";
        var generator = new QRCodeGenerator();
        QRCodeData qrCodeData = generator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        using var pngByteQrCode = new PngByteQRCode(qrCodeData);
        byte[] data = pngByteQrCode.GetGraphic(20);
        QrCode = Convert.ToBase64String(data);
        return Page();
    }
}
