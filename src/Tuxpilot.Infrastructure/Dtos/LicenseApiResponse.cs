namespace Tuxpilot.Infrastructure.Dtos;

/// <summary>
/// Réponse de l'API (mapping JSON Python)
/// </summary>
public class LicenseApiResponse
{
    public bool Valid { get; set; }
    public string? Type { get; set; }
    public List<string>? Features { get; set; }
    public string? Key { get; set; }
    public string? Error { get; set; }
    public bool Stored { get; set; }
    public string? ActivatedAt { get; set; }
    public string? ExpiresAt { get; set; }
    public string? UserId { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public bool Success { get; set; }
}