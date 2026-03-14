using Sqids;

namespace TinyUrlService.Utils;

public class SqidsGenerator
{
    private readonly SqidsEncoder<long> _sqids;

    public SqidsGenerator()
    {
        _sqids = new SqidsEncoder<long>(new()
        {
            MinLength = 6
        });
    }

    public string Encode(long id)
    {
        return _sqids.Encode(new[] { id });
    }

    public long Decode(string shortId)
    {
        var decoded = _sqids.Decode(shortId);
        return decoded.FirstOrDefault();
    }
}