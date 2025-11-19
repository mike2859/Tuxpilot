using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.UI.ViewModels;


/// <summary>
/// ViewModel pour l'assistant IA
/// </summary>
public partial class AssistantIAViewModel : ViewModelBase
{
    private readonly IServiceAssistantIA _serviceIA;
    
    [ObservableProperty]
    private ObservableCollection<ChatMessageViewModel> _messages = new();
    
    [ObservableProperty]
    private string _questionUtilisateur = string.Empty;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private bool _isStreaming;
    
    [ObservableProperty]
    private string _messagePlaceholder = "Posez une question sur Linux, Fedora, les commandes...";
    
    public AssistantIAViewModel(IServiceAssistantIA serviceIA)
    {
        _serviceIA = serviceIA;
        
        // Message de bienvenue
        Messages.Add(new ChatMessageViewModel(
            "👋 Bonjour ! Je suis votre assistant Linux.\n\n" +
            "Je peux vous aider avec :\n" +
            "• Commandes Linux\n" +
            "• Gestion des paquets (DNF, APT)\n" +
            "• Dépannage système\n" +
            "• Explications des concepts\n\n" +
            "Posez-moi vos questions !",
            isUser: false
        ));
    }
    [RelayCommand]
    private async Task EnvoyerQuestionAsync()
    {
        // Vérifier que la question n'est pas vide
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

            // 🆕 VERSION SIMPLE SANS STREAMING
            var reponse = await _serviceIA.DemanderAsync(question);

            // Remplacer le message de chargement par la réponse
            Messages.Remove(messageIA);
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
    
    [RelayCommand]
    private void EffacerHistorique()
    {
        Messages.Clear();
        
        // Remettre le message de bienvenue
        Messages.Add(new ChatMessageViewModel(
            "👋 Historique effacé ! Posez-moi une nouvelle question.",
            isUser: false
        ));
    }
    
    /// <summary>
    /// Questions suggérées rapides
    /// </summary>
    [RelayCommand]
    private void PoserQuestionRapide(string question)
    {
        QuestionUtilisateur = question;
        _ = EnvoyerQuestionAsync();
    }
}