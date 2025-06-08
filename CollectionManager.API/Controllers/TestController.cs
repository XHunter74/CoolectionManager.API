using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xhunter74.CollectionManager.Shared.Services.Interfaces;

namespace xhunter74.CollectionManager.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;
    private readonly IEmailService _emailService;

    public TestController(
        ILogger<TestController> logger,
        IEmailService emailService
        )
    {
        _logger = logger;
        _emailService = emailService;
    }

    [HttpGet]
    public async Task<IActionResult> Test()
    {
        await _emailService.SendEmailAsync("xhunter74@gmail.com", "Test email", "<p>Test</p>");
        return Ok();
    }
}
