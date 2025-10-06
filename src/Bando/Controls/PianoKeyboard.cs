namespace Bando.Controls;

using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Melanchall.DryWetMidi.MusicTheory;
using Note = Bando.Core.Midi.Note;
public class PianoKeyboard : Panel
{
    private readonly int _whiteKeyCount = 52;
    private Grid _whiteKeyGrid = new();
    private Canvas _blackKeyCanvas = new() { Height = 50, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, };
    internal bool MouseDown = false;
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        DrawKeys();
    }
    private void DrawKeys()
    {
        Background = Avalonia.Media.Brushes.Black;
        Grid keyboardGrid = new();
        keyboardGrid.Children.Add(_whiteKeyGrid);
        keyboardGrid.Children.Add(_blackKeyCanvas);
        _whiteKeyGrid.ColumnDefinitions = new(string.Concat(Enumerable.Repeat("*,", _whiteKeyCount)).TrimEnd(','));
        NoteName[] whiteKeyProgression = { NoteName.A, NoteName.B, NoteName.C, NoteName.D, NoteName.E, NoteName.F, NoteName.G };
        for (int i = 0; i < _whiteKeyCount; i++)
        {
            var whiteKey = new WhitePianoKey()
            {
                NoteName = whiteKeyProgression[i % whiteKeyProgression.Length],
                Octave = (i / whiteKeyProgression.Length) + (i % whiteKeyProgression.Length >= 2 ? 1 : 0)
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
            NoteName[] twoBlacksProgression = new[] { NoteName.CSharp, NoteName.DSharp };
            NoteName[] threeBlacksProgression = new[] { NoteName.FSharp, NoteName.GSharp, NoteName.ASharp };
            int octave = 0;
            foreach (var whiteKey in whiteKeys)
            {
                //render black keys in segments
                int blackCount = 0;
                if (whiteKey.NoteName == NoteName.C)
                    blackCount = 2;
                else if (whiteKey.NoteName == NoteName.F)
                    blackCount = 3;
                else if (whiteKey.NoteName == NoteName.A && whiteKey.Octave == 0)
                    blackCount = 1;

                if (blackCount > 0 && whiteKey.Octave != 8)
                {
                    double blackWidth = whiteKey.Bounds.Width * 0.6;
                    double blackHeight = whiteKey.Bounds.Height * 0.65;
                    double spacing = whiteKey.Bounds.Width * 0.4;
                    double push = whiteKey.Bounds.Width - blackWidth / 2;
                    for (int i = 0; i < blackCount; i++)
                    {
                        var mkey = (blackCount == 2) ? twoBlacksProgression[i] :
                            (blackCount == 3) ? threeBlacksProgression[i] : NoteName.ASharp;
                        var key = new BlackPianoKey { Width = blackWidth, Height = blackHeight, NoteName = mkey, Octave = octave };
                        Canvas.SetLeft(key, offset + push);
                        _blackKeyCanvas.Children.Add(key);
                        push += blackWidth + spacing;
                        if (mkey == NoteName.ASharp) octave++;
                    }
                }
                offset += whiteKey.Bounds.Width;
            }
        }
    }

    public void IlluminateKey(Bando.Core.Midi.Note note, bool on)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_blackKeyCanvas is null || _whiteKeyGrid is null)
                return;
            bool isBlackKey = note.NoteName is NoteName.CSharp or NoteName.DSharp or NoteName.FSharp or NoteName.GSharp or NoteName.ASharp;
            var source = isBlackKey ? _blackKeyCanvas.Children : _whiteKeyGrid.Children;
            var key = source
                .OfType<PianoKey>()
                .FirstOrDefault(p => p.NoteName == note.NoteName && p.Octave == note.Octave);
            if (key is null)
            {
                Console.WriteLine($"No key found for {note.NoteName}{note.Octave}");
                return;
            }
            key.SetPseudoClasses(on);
        });
    }

    public static readonly RoutedEvent<KeyPressedEventArgs> KeyPressedEvent =
        RoutedEvent.Register<PianoKeyboard, KeyPressedEventArgs>(nameof(KeyPressed), RoutingStrategies.Bubble);

    public event EventHandler<KeyPressedEventArgs> KeyPressed
    {
        add => AddHandler(KeyPressedEvent, value);
        remove => RemoveHandler(KeyPressedEvent, value);
    }

}
public class KeyPressedEventArgs : RoutedEventArgs
{
    public Note Note { get; set; }
    public KeyPressedEventArgs(RoutedEvent routedEvent, Note note) : base(routedEvent)
    {
        Note = note;
    }
}

[PseudoClasses(":keyOn")]
public abstract class PianoKey : Panel
{
    public NoteName NoteName { get; set; }
    public int Octave { get; set; }
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Draw();
    }
    protected abstract void Draw();
    public void SetPseudoClasses(bool keyOn)
    {
        PseudoClasses.Set(":keyOn", keyOn);
    }
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var keyboard = this.FindAncestorOfType<PianoKeyboard>();
        if (keyboard is null) return;
        keyboard.MouseDown = true;
        keyboard.RaiseEvent(new KeyPressedEventArgs(PianoKeyboard.KeyPressedEvent, GetNote()));
        PseudoClasses.Set(":pressed", true);
        e.Pointer.Capture(null); //allow other events
    }
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        PseudoClasses.Set(":pressed", false);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        var keyboard = this.FindAncestorOfType<PianoKeyboard>();
        if (keyboard is null) return;
        keyboard.MouseDown = false;
        PseudoClasses.Set(":pressed", false);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        var keyboard = this.FindAncestorOfType<PianoKeyboard>();
        if (keyboard is null) return;
        if (keyboard.MouseDown)
        {
            keyboard.RaiseEvent(new KeyPressedEventArgs(PianoKeyboard.KeyPressedEvent, GetNote()));
            PseudoClasses.Set(":pressed", true);
        }
        base.OnPointerEntered(e);
    }
    public Bando.Core.Midi.Note GetNote() => new() { Octave = Octave, NoteName = NoteName };
}

public class WhitePianoKey : PianoKey
{
    protected override void Draw()
    {
        var keyBorder = new Border
        {
            BorderBrush = Avalonia.Media.Brushes.Black,
            BorderThickness = new(0, 0, 1, 1),
            CornerRadius = new(0, 0, 8, 8)
        };
        Children.Add(keyBorder);
    }
}

public class BlackPianoKey : PianoKey
{
    protected override void Draw()
    {
        var keyBorder = new Border
        {
            BorderThickness = new(1),
            CornerRadius = new(0, 0, 2, 2),
            BoxShadow = new(new()
            {
                Color = Color.Parse("#80000000"), 
                Blur = 2,
                OffsetX = 0,
                OffsetY = 0,
            }),
        };
        Children.Add(keyBorder);
    }
}

