using MailKitSimplified.Sender.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace OliverBooth.Controllers;

[Controller]
[Route("contact/submit")]
public class ContactController : Controller
{
    private readonly ILogger<ContactController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IConfigurationSection _destination;
    private readonly IConfigurationSection _sender;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContactController" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration">The configuration.</param>
    public ContactController(ILogger<ContactController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        IConfigurationSection mailConfiguration = configuration.GetSection("Mail");
        _destination = mailConfiguration.GetSection("Destination");
        _sender = mailConfiguration.GetSection("Sender");
    }

    [HttpGet("{_?}")]
    public IActionResult OnGet(string _)
    {
        _logger.LogWarning("Method GET for endpoint {Path} is not supported!", Request.Path);
        return RedirectToPage("/Contact/Index");
    }

    [HttpPost("other")]
    public async Task<IActionResult> HandleForm()
    {
        if (!Request.HasFormContentType)
        {
            return RedirectToPage("/Contact/Index");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogError("Captcha validation failed");
            TempData["Success"] = false;
            return RedirectToPage("/Contact/Result");
        }

        IFormCollection form = Request.Form;
        StringValues name = form["name"];
        StringValues email = form["email"];
        StringValues subject = form["subject"];
        StringValues message = form["message"];

        await using SmtpSender sender = CreateSender();
        try
        {
            await sender.WriteEmail
                .To("Oliver Booth", _destination.Get<string>())
                .From("Contact via Website", _sender.Get<string>())
                .ReplyTo(name, email)
                .Subject(subject)
                .BodyText(message)
                .SendAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to send email");
            TempData["Success"] = false;
            return RedirectToPage("/Contact/Result");
        }

        TempData["Success"] = true;
        return RedirectToPage("/Contact/Result");
    }

    private SmtpSender CreateSender()
    {
        IConfigurationSection mailSection = _configuration.GetSection("Mail");
        string? mailServer = mailSection.GetSection("Server").Value;
        string? mailUsername = mailSection.GetSection("Username").Value;
        string? mailPassword = mailSection.GetSection("Password").Value;
        ushort port = mailSection.GetSection("Port").Get<ushort>();

        var sender = SmtpSender.Create(mailServer, port);
        sender.SetCredential(mailUsername, mailPassword);
        return sender;
    }
}
