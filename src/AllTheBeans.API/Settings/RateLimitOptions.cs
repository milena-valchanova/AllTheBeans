namespace AllTheBeans.API.Settings;

public class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    public int PermitLimit { get; set; } = 5;
    public int QueueLimit { get; set; } = 10;
}
