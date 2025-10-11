using Avalonia;
using Avalonia.Controls;
using Bando.ViewModels;

namespace Bando.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel()
        {
            Keyboard = keyboard,
            SvgImage = svgImage,
        };
    }
}
