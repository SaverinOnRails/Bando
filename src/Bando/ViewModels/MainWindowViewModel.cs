using Bando.Core.Midi;
namespace Bando.ViewModels;


public partial class MainWindowViewModel : ViewModelBase
{
    private MidiPlayer _midiPlayer = new();
    public MainWindowViewModel()
    {
        _midiPlayer.LoadMidiFile("/home/noble/Midis/Marshmello ft. Bastille - Happier.mid");
    }
    public string Greeting { get; } = "Welcome to Avalonia!";
}
