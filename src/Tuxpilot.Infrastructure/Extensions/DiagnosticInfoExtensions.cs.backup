using Tuxpilot.Core.Entities;
using Tuxpilot.Infrastructure.Dtos;

namespace Tuxpilot.Infrastructure.Extensions;


/// <summary>
/// Extensions pour mapper les DTOs de diagnostic
/// </summary>
public static class DiagnosticInfoExtensions
{
    public static DiagnosticInfo ToEntity(this DiagnosticInfoDto dto)
    {
        return new DiagnosticInfo
        {
            Timestamp = dto.Timestamp,
            ScoreSante = dto.ScoreSante,
            EtatGlobal = dto.EtatGlobal,
            MessageGlobal = dto.MessageGlobal,
            NombreServicesErreur = dto.Services.NombreErreurs,
            Services = dto.Services.Services.Select(s => s.ToEntity()).ToList(),
            NombreLogs = dto.Logs.NombreLogs,
            Logs = dto.Logs.Logs.Select(l => l.ToEntity()).ToList(),
            Disque = dto.Disque.ToEntity(),
            TopCpu = dto.Processus.TopCpu.Select(p => p.ToEntity()).ToList(),
            TopRam = dto.Processus.TopRam.Select(p => p.ToEntity()).ToList(),
            Erreur = dto.Erreur
        };
    }
    
    public static ServiceInfo ToEntity(this ServiceInfoDto dto)
    {
        return new ServiceInfo
        {
            Nom = dto.Nom,
            Etat = dto.Etat,
            Description = dto.Description
        };
    }
    
    public static LogEntry ToEntity(this LogEntryDto dto)
    {
        return new LogEntry
        {
            Timestamp = dto.Timestamp,
            Service = dto.Service,
            Message = dto.Message
        };
    }
    
    public static ProcessInfo ToEntity(this ProcessInfoDto dto)
    {
        return new ProcessInfo
        {
            Nom = dto.Nom,
            Utilisateur = dto.Utilisateur,
            Cpu = dto.Cpu,
            Ram = dto.Ram,
            Pid = dto.Pid
        };
    }
    
    public static DiskInfo ToEntity(this DiskInfoDto dto)
    {
        return new DiskInfo
        {
            Partition = dto.Partition,
            Taille = dto.Taille,
            Utilise = dto.Utilise,
            Disponible = dto.Disponible,
            Pourcentage = dto.Pourcentage
        };
    }
}