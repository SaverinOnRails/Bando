using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
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
    public double tempoScale = 0;
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
        _midiPlayer.LoadMidiFile("/home/noble/Midis/Beatles - Hey Jude.mid");
        _midiPlayer.MidiKeyOn += MidiKeyOn;
        _midiPlayer.MidiKeyOff += MidiKeyOff;
        _midiPlayer.MidiPlaybackLocationChanged += MidiPlaybackLocationChanged;
        _midiPlayer.TurnOffAllNotes += TurnOffAllNotes;

    }

    private void TurnOffAllNotes(object? sender, EventArgs e)
    {
        if (Keyboard is null) return;
        Keyboard.KeyOffAll();
    }

    private void MidiPlaybackLocationChanged(object sender, double newLocation)
    {
        _updatingPos = true;
        Location = newLocation;
        Duration = _midiPlayer.PlaybackDuration;
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
    public async void SelectFile()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        if (desktop.MainWindow is null)
            return;

        var files = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("filesv")
        {
            Patterns = new List<string>()
            {
                "*.mid", "*.midi"
            }
        }}
        });

        if (files.Count >= 1)
        {
            var localPath = files[0].TryGetLocalPath();
            if (localPath is not null)
                _midiPlayer.LoadMidiFile(localPath);
        }
    }
    public void IncreaseTempo()
    {
        _midiPlayer.IncreaseTempoScale(0.1);
        TempoScale = Math.Round(_midiPlayer.TempoScale, 2);
    }

    public void DecreaseTempo()
    {
        _midiPlayer.DecreaseTempoScale(0.1);
        TempoScale = Math.Round(_midiPlayer.TempoScale, 2);
    }
    public void Pause() => _midiPlayer.Pause();
    public void Play() => _midiPlayer.Play();
    public string Greeting { get; } = "Welcome to Avalonia!";
}
