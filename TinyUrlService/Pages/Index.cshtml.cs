using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TinyUrlService.Services;

public class IndexModel : PageModel
{
    private readonly UrlService _service;
    private readonly IHttpContextAccessor _context;

    public string? ShortUrl { get; set; }

    public IndexModel(UrlService service, IHttpContextAccessor context)
    {
        _service = service;
        _context = context;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string LongUrl)
    {
        if (string.IsNullOrEmpty(LongUrl))
            return Page(); // just redisplay if empty

        var shortId = await _service.CreateShortUrl(LongUrl);

        var req = _context.HttpContext!.Request;
        var baseUrl = $"{req.Scheme}://{req.Host}";

        ShortUrl = $"{baseUrl}/{shortId}";

        return Page(); // <-- this is key
    }
}