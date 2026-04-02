using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TinyUrlService.Services;

public class IndexModel : PageModel
{
    private readonly UrlService _service;
    private readonly IHttpContextAccessor _context;

    [BindProperty] // <-- THIS is the key
    public string? LongUrl { get; set; }

    public string? ShortUrl { get; set; }

    public IndexModel(UrlService service, IHttpContextAccessor context)
    {
        _service = service;
        _context = context;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(LongUrl))
            return Page();

        var shortId = await _service.CreateShortUrl(LongUrl);

        var req = _context.HttpContext!.Request;
        var baseUrl = $"{req.Scheme}://{req.Host}";

        ShortUrl = $"{baseUrl}/r/{shortId}";
        return Page();
    }
}