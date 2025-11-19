namespace Tuxpilot.Infrastructure.Dtos;


public class ProcessusResultDto
{
    public List<ProcessInfoDto> TopCpu { get; set; } = new();
    public List<ProcessInfoDto> TopRam { get; set; } = new();
}