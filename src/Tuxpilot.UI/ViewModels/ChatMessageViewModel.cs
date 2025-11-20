using CommunityToolkit.Mvvm.ComponentModel;
using Tuxpilot.UI.Models;

namespace Tuxpilot.UI.ViewModels;


/// <summary>
/// ViewModel pour un message du chat
/// </summary>
public partial class ChatMessageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _texte = string.Empty;
    
    [ObservableProperty]
    private bool _isUser;
    
    [ObservableProperty]
    private string _icone = "🤖";
    
    [ObservableProperty]
    private string _backgroundColor = "#F3F4F6";
    
    [ObservableProperty]
    private string _textColor = "#111827";
    
    [ObservableProperty]
    private bool _hasAction;
    
    [ObservableProperty]
    private AssistantAction? _action;
    
    [ObservableProperty]
    private bool _actionExecuted;
    
    [ObservableProperty]
    private string _actionResult = string.Empty;
    public ChatMessageViewModel(string texte, bool isUser)
    {
        Texte = texte;
        IsUser = isUser;
        
        if (isUser)
        {
            Icone = "👤";
            BackgroundColor = "#DBEAFE";
            TextColor = "#111827";
        }
        else
        {
            Icone = "🤖";
            BackgroundColor = "#F3F4F6";
            TextColor = "#374151";
        }
    }
}