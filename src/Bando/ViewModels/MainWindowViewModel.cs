using Bando.Controls;
using Bando.Core.Midi;
using CommunityToolkit.Mvvm.ComponentModel;
namespace Bando.ViewModels;
public partial class MainWindowViewModel : ViewModelBase
{
    private MidiPlayer _midiPlayer = new();
    private PianoKeyboard? _keyboard = null;

    [ObservableProperty]
    public double duration;

    [ObservableProperty]
    public double location;

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
        _midiPlayer.LoadMidiFile("/home/noble/Downloads/elasticheat.mid");
        _midiPlayer.MidiKeyOn += MidiKeyOn;
        _midiPlayer.MidiKeyOff += MidiKeyOff;
        _midiPlayer.MidiPlaybackLocationChanged += MidiPlaybackLocationChanged;
        Duration = _midiPlayer.PlaybackDuration;
    }

    private void MidiPlaybackLocationChanged(object sender, double newLocation)
    {
        Location = newLocation;
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
    public void Pause() => _midiPlayer.Pause();
    public void Play() => _midiPlayer.Play();
    public string Greeting { get; } = "Welcome to Avalonia!";
}
