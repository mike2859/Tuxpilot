namespace Tuxpilot.Infrastructure.Dtos;

public class ProcessInfoDto
{
    public string Nom { get; set; } = string.Empty;
    public string Utilisateur { get; set; } = string.Empty;
    public string Cpu { get; set; } = string.Empty;
    public string Ram { get; set; } = string.Empty;
    public string Pid { get; set; } = string.Empty;
}