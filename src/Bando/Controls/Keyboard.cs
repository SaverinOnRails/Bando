namespace Bando.Controls;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

public class PianoKeyboard : Panel
{
    private readonly int _whiteKeyCount = 52;
    private Grid _whiteKeyGrid = new();
    private Canvas _blackKeyCanvas = new() { Height = 50, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, };
    private static readonly MusicKey[] WhiteKeyPattern =
    {
        MusicKey.A, MusicKey.B, MusicKey.C, MusicKey.D, MusicKey.E, MusicKey.F, MusicKey.G
    };
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        DrawKeys();
    }
    private void DrawKeys()
    {
        Grid keyboardGrid = new();
        keyboardGrid.Children.Add(_whiteKeyGrid);
        keyboardGrid.Children.Add(_blackKeyCanvas);
        _whiteKeyGrid.ColumnDefinitions = new(string.Concat(Enumerable.Repeat("*,", _whiteKeyCount)).TrimEnd(','));
        for (int i = 0; i < _whiteKeyCount; i++)
        {
            var whiteKey = new WhitePianoKey()
            {
                MusicKey = WhiteKeyPattern[i % WhiteKeyPattern.Length],
                Octave = (i / WhiteKeyPattern.Length) + (i % WhiteKeyPattern.Length >= 2 ? 1 : 0)
            };
            Grid.SetColumn(whiteKey, i);
            _whiteKeyGrid.Children.Add(whiteKey);
        }
        _whiteKeyGrid.PropertyChanged += WhiteKeyGridPropertyChanged;
        Children.Add(keyboardGrid);
    }

    private void WhiteKeyGridPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is null) return;
        if (e.Property == BoundsProperty)
        {
            _blackKeyCanvas.Children.Clear();
            var whiteKeys = _whiteKeyGrid.Children.OfType<WhitePianoKey>().ToList();
            double offset = 0;
            foreach (var whiteKey in whiteKeys)
            {
                int blackCount = 0;
                if (whiteKey.MusicKey == MusicKey.C)
                    blackCount = 2;
                else if (whiteKey.MusicKey == MusicKey.F)
                    blackCount = 3;
                else if (whiteKey.MusicKey == MusicKey.A && whiteKey.Octave == 0)
                    blackCount = 1;

                if (blackCount > 0 && whiteKey.Octave != 8)
                {
                    double blackWidth = whiteKey.Bounds.Width * 0.6;
                    double spacing = whiteKey.Bounds.Width * 0.4;
                    double push = whiteKey.Bounds.Width - blackWidth / 2;

                    for (int i = 0; i < blackCount; i++)
                    {
                        var key = new BlackPianoKey { Width = blackWidth };
                        Canvas.SetLeft(key, offset + push);
                        _blackKeyCanvas.Children.Add(key);
                        push += blackWidth + spacing;
                    }
                }
                offset += whiteKey.Bounds.Width;
            }
        }
    }
}


public abstract class PianoKey : Panel
{
    public MusicKey MusicKey { get; set; }
    public int Octave { get; set; }
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Draw();
    }
    protected abstract void Draw();
}

public class WhitePianoKey : PianoKey
{
    protected override void Draw()
    {
        var keyBorder = new Border
        {
            BorderBrush = Avalonia.Media.Brushes.Black,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Background = Avalonia.Media.Brushes.White,
        };
        Children.Add(keyBorder);
    }
}

public class BlackPianoKey : PianoKey
{
    protected override void Draw()
    {
        Height = 50;
        Background = Avalonia.Media.Brushes.Black;
    }
}

public enum MusicKey
{
    A, B, C, D, E, F, G
}
