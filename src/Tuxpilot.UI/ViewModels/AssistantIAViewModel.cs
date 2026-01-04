using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;
using Tuxpilot.UI.Models;

namespace Tuxpilot.UI.ViewModels;



/// <summary>
/// ViewModel pour l'assistant IA
/// </summary>
public partial class AssistantIAViewModel : ViewModelBase
{
    private readonly IServiceAssistantIA _serviceIA;
    private readonly IServiceCommandes _serviceCommandes;
    private readonly IServiceHistorique _serviceHistorique;
    private readonly IServiceContexteSysteme _contexte;

    [ObservableProperty]
    private ObservableCollection<ChatMessageViewModel> _messages = new();
    
    [ObservableProperty]
    private string _questionUtilisateur = string.Empty;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private bool _isStreaming;
    
    [ObservableProperty]
    private string _messagePlaceholder = "Posez une question ou demandez une action (ex: Installe VLC)";
    
    [ObservableProperty] private string _etatContexte = "Contexte : non chargé";
    [ObservableProperty] private DateTimeOffset? _contexteDate;

    public AssistantIAViewModel(
        IServiceAssistantIA serviceIA,
        IServiceCommandes serviceCommandes,
        IServiceHistorique serviceHistorique,
        IServiceContexteSysteme contexte)
    {
        _serviceIA = serviceIA;
        _serviceCommandes = serviceCommandes;
        _serviceHistorique = serviceHistorique;
        _contexte = contexte;
        
        // Message de bienvenue
        Messages.Add(new ChatMessageViewModel(
            "👋 Bonjour ! Je suis votre assistant Linux.\n\n" +
            "Je peux vous aider avec :\n" +
            "• Commandes Linux\n" +
            "• Gestion des paquets (DNF, APT)\n" +
            "• Dépannage système\n" +
            "• Explications des concepts\n\n" +
            "💡 NOUVEAU : Je peux aussi exécuter des actions pour vous !\n" +
            "Exemples : 'Installe VLC', 'Supprime Firefox', 'Redémarre Apache'\n\n" +
            "Posez-moi vos questions !",
            isUser: false
        ));
    }

    [RelayCommand]
    private async Task RafraichirContexteAsync()
    {
        try
        {
            IsLoading = true;
            EtatContexte = "🔄 Rafraîchissement du contexte…";

            var snap = await _contexte.GetSnapshotAsync(forceRefresh: true);
            ContexteDate = snap.CapturedAt;
            EtatContexte = $"✅ Contexte à jour ({snap.CapturedAt.LocalDateTime:HH:mm})";
        }
        catch (Exception ex)
        {
            EtatContexte = $"❌ Contexte indisponible : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task EnvoyerQuestionAsync()
    {
        if (string.IsNullOrWhiteSpace(QuestionUtilisateur))
            return;

        var question = QuestionUtilisateur.Trim();
        
        // Ajouter la question de l'utilisateur
        Messages.Add(new ChatMessageViewModel(question, isUser: true));
        
        // Vider le champ de saisie
        QuestionUtilisateur = string.Empty;
        
        // Message "En attente..."
        var messageIA = new ChatMessageViewModel("🔄 Génération de la réponse...", isUser: false);
        Messages.Add(messageIA);

        try
        {
            IsStreaming = true;

            var snap = await _contexte.GetSnapshotAsync(forceRefresh: ShouldForceRefresh(question));
            ContexteDate = snap.CapturedAt;
            EtatContexte = $"✅ Contexte prêt ({snap.CapturedAt.LocalDateTime:HH:mm})";

            var contexteJson = JsonSerializer.Serialize(snap, new JsonSerializerOptions { WriteIndented = false });

            var reponse = await _serviceIA.DemanderAsync(question, contexteJson);

            // Demander à l'IA
           // var reponse = await _serviceIA.DemanderAsync(question);

            // Retirer le message de chargement
            Messages.Remove(messageIA);

            // 🆕 DÉTECTER SI C'EST UNE ACTION
            if (reponse.TrimStart().StartsWith("{"))
            {
                // Essayer de parser comme JSON
                try
                {
                    var action = JsonSerializer.Deserialize<AssistantAction>(reponse);
                    
                    if (action != null && action.IsValid)
                    {
                        // C'est une action !
                        var actionMessage = new ChatMessageViewModel(
                            action.Explanation,
                            isUser: false
                        )
                        {
                            HasAction = true,
                            Action = action
                        };
                        
                        Messages.Add(actionMessage);
                        return;
                    }
                }
                catch (JsonException)
                {
                    // Pas du JSON valide, traiter comme texte normal
                }
            }

            // Réponse texte normale
            Messages.Add(new ChatMessageViewModel(reponse, isUser: false));
        }
        catch (Exception ex)
        {
            Messages.Remove(messageIA);
            Messages.Add(new ChatMessageViewModel($"❌ Erreur : {ex.Message}", isUser: false));
        }
        finally
        {
            IsStreaming = false;
        }
    }

    private static bool ShouldForceRefresh(string q)
    {
        q = q.ToLowerInvariant();
        return q.Contains("analyse") || q.Contains("mon système") || q.Contains("sécur") || q.Contains("audit") || q.Contains("ports") || q.Contains("firewall");
    }
    
    /// <summary>
    /// Exécute l'action détectée par l'IA
    /// </summary>
    [RelayCommand]
    private async Task ExecuterActionAsync(ChatMessageViewModel message)
    {
        if (message.Action == null || message.ActionExecuted)
            return;

        try
        {
            // Marquer comme exécuté
            message.ActionExecuted = true;
            
            // Créer un message pour les logs
            var logsMessage = new ChatMessageViewModel(
                $"🔄 Exécution de : {message.Action.Command}\n\n",
                isUser: false
            );
            Messages.Add(logsMessage);

            // Exécuter avec streaming des logs
            await _serviceCommandes.ExecuterCommandeAvecLogsAsync(
                message.Action.Command,
                log =>
                {
                    // Dispatcher pour UI thread
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        logsMessage.Texte += log;
                    });
                },
                message.Action.NeedsSudo
            );

            // Mettre à jour le résultat
            message.ActionResult = "✅ Action terminée !";
            
            await _serviceHistorique.AjouterActionAsync(
                TypeAction.AI,
                $"Commande IA exécutée : {message.Action.Command}",
                true
            );
        }
        catch (Exception ex)
        {
            message.ActionResult = $"❌ Erreur : {ex.Message}";
            
            await _serviceHistorique.AjouterActionAsync(
                TypeAction.AI,
                $"Échec commande IA : {message.Action.Command}",
                false
            );
        }
    }
    
    /// <summary>
    /// Refuse l'action proposée
    /// </summary>
    [RelayCommand]
    private void RefuserAction(ChatMessageViewModel message)
    {
        if (message.Action == null)
            return;
    
        // Marquer comme exécuté pour cacher les boutons
        message.ActionExecuted = true;
        message.ActionResult = "Action annulée par l'utilisateur";
    }
    
    [RelayCommand]
    private void EffacerHistorique()
    {
        Messages.Clear();
        
        Messages.Add(new ChatMessageViewModel(
            "👋 Historique effacé ! Posez-moi une nouvelle question.",
            isUser: false
        ));
    }
    
    [RelayCommand]
    private void PoserQuestionRapide(string question)
    {
        QuestionUtilisateur = question;
        _ = EnvoyerQuestionAsync();
    }
}