namespace Tuxpilot.Infrastructure.Dtos;

public class ServicesListDto
{
    public List<ServiceSystemdDto> Services { get; set; } = new();
    public int Count { get; set; }
    public string? Error { get; set; }
}