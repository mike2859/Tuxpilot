using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Tuxpilot.UI.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        AvaloniaXamlLoader.Load(this);
        
        // Créer le ViewModel
        DataContext = new ViewModels.DashboardViewModel();
    }
}