using Avalonia.Input;
namespace Bando.Controls;

//No space focus button
public class Button : Avalonia.Controls.Button
{
    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                OnClick();
                e.Handled = true;
                break;
            case Key.Escape when Flyout != null:
                CloseFlyout();
                break;
        }
        // base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
    }
}
