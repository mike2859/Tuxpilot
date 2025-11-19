using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Tuxpilot.UI.ViewModels;

namespace Tuxpilot.UI.Views;


public partial class MisesAJourView : UserControl
{
    public MisesAJourView()
    {
        AvaloniaXamlLoader.Load(this); 
        
        // S'abonner aux changements de logs pour auto-scroll
        DataContextChanged += (s, e) =>
        {
            if (DataContext is MisesAJourViewModel vm)
            {
                vm.Logs.CollectionChanged += (_, __) =>
                {
                    // Auto-scroll vers le bas quand un nouveau log arrive
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        if (this.FindControl<ScrollViewer>("LogsScrollViewer") is { } scrollViewer)
                        {
                            scrollViewer.ScrollToEnd();
                        }
                    });
                };
            }
        };
    }
}