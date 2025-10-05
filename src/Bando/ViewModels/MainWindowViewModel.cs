using System;
using Avalonia.Threading;
using Bando.Controls;
using Bando.Core.Midi;
namespace Bando.ViewModels;


public partial class MainWindowViewModel : ViewModelBase
{
    private MidiPlayer _midiPlayer = new();
    public PianoKeyboard? Keyboard { get; set; } = null;  //TEMPORARY HACK

    public MainWindowViewModel()
    {
        _midiPlayer.LoadMidiFile("/home/noble/Midis/Experience_-_Ludovico_Einaudi.mid");
        _midiPlayer.MidiKeyOn += MidiKeyOn;
        _midiPlayer.MidiKeyOff += MidiKeyOff;
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
