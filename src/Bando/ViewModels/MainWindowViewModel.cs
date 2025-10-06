using Bando.Controls;
using Bando.Core.Midi;
namespace Bando.ViewModels;
public partial class MainWindowViewModel : ViewModelBase
{
    private MidiPlayer _midiPlayer = new();
    private PianoKeyboard? _keyboard = null;

    public PianoKeyboard? Keyboard
    {
        get => _keyboard;
        set
        {
            _keyboard = value;
            value!.KeyPressed += KeypressedOnKeyboard;
        }
    }
    public MainWindowViewModel()
    {
        _midiPlayer.LoadMidiFile("/home/noble/Midis/Sting - Shape Of My Heart.mid");
        _midiPlayer.MidiKeyOn += MidiKeyOn;
        _midiPlayer.MidiKeyOff += MidiKeyOff;
    }

    private void KeypressedOnKeyboard(object? sender, KeyPressedEventArgs e)
    {
        _midiPlayer.SynthPlayNote(e.Note);
    }

    private void MidiKeyOff(object sender, Note e)
    {
        Keyboard!.IlluminateKey(e, false);
    }

    private void MidiKeyOn(object sender, Note e)
    {
        Keyboard!.IlluminateKey(e, true);
    }

    public void Start()
    {
        _midiPlayer.StartPlayback();
    }
    public string Greeting { get; } = "Welcome to Avalonia!";

}
