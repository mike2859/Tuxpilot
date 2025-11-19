using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Tuxpilot.UI.ViewModels;

namespace Tuxpilot.UI.Views;


public partial class AssistantIAView : UserControl
{
    public AssistantIAView()
    {
        AvaloniaXamlLoader.Load(this);
        
        // Auto-scroll des messages
        DataContextChanged += (s, e) =>
        {
            if (DataContext is AssistantIAViewModel vm)
            {
                vm.Messages.CollectionChanged += (_, __) =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        if (this.FindControl<ScrollViewer>("MessagesScrollViewer") is { } scrollViewer)
                        {
                            scrollViewer.ScrollToEnd();
                        }
                    });
                };
            }
        };
    }
}