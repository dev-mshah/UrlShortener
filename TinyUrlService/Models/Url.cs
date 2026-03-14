namespace TinyUrlService.Models;

public class Url
{
    public long Id { get; set; }

    public string ShortId { get; set; } = string.Empty;

    public string LongUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}