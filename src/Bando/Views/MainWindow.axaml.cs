using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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
            Sheet = sheet,
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Space) e.Handled = true;
    }
}
