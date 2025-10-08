using System;
using Bando.Controls;
using Bando.Core.Midi;
using CommunityToolkit.Mvvm.ComponentModel;
namespace Bando.ViewModels;
public partial class MainWindowViewModel : ViewModelBase
{
    private MidiPlayer _midiPlayer = new();
    private PianoKeyboard? _keyboard = null;

    private bool _updatingPos = false;
    private double _location = 0;
    [ObservableProperty]
    public double duration;

    public double Location
    {
        get => _location;
        set
        {
            if (!_updatingPos)
            {
                _midiPlayer.SeekTo((long)value);
            }
            else
            {
                SetProperty(ref _location, value);
            }
        }
    }

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
        _midiPlayer.LoadMidiFile("/home/noble/Midis/River_Flows_In_You.mid");
        _midiPlayer.MidiKeyOn += MidiKeyOn;
        _midiPlayer.MidiKeyOff += MidiKeyOff;
        _midiPlayer.MidiPlaybackLocationChanged += MidiPlaybackLocationChanged;
        _midiPlayer.TurnOffAllNotes += TurnOffAllNotes;
        Duration = _midiPlayer.PlaybackDuration;
    }

    private void TurnOffAllNotes(object? sender, EventArgs e)
    {
        if(Keyboard is null) return;
        Keyboard.KeyOffAll();
    }

    private void MidiPlaybackLocationChanged(object sender, double newLocation)
    {
        _updatingPos = true;
        Location = newLocation;
        _updatingPos = false;
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
