namespace Bando.Core.Midi;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;

public class MidiPlayer : IDisposable
{
    private MidiFile? _midiFile = null;
    private List<TimedEvent>? _timedEvents = null;
    private FluidSynth _synth = new();
    private Task? _playbackTask = null;
    private bool _disposed = false;
    public delegate void MidiKeyEventHandler(object sender, Note e);
    public event MidiKeyEventHandler? MidiKeyOn;
    public event MidiKeyEventHandler? MidiKeyOff;

    public void Dispose()
    {
        Dispose(true);
    }
    protected virtual void Dispose(bool dispose)
    {
        if (!_disposed)
        {
            _synth.Dispose();
            _disposed = true;
        }
    }
    public void LoadMidiFile(string file)
    {
        _midiFile = MidiFile.Read(file);
        _timedEvents = _midiFile.GetTimedEvents().ToList();
    }
    public void StartPlayback()
    {
        _playbackTask = Task.Run(() => PlaybackTask());
    }
    private async Task PlaybackTask()
    {
        if (_timedEvents is null || _midiFile is null) return;
        var tempoMap = _midiFile.GetTempoMap();
        long previousTime = 0;
        var now = DateTime.Now;
        foreach (var timedEvent in _timedEvents)
        {
            long timeDelta = timedEvent.Time - previousTime;
            previousTime = timedEvent.Time;
            var delay = TimeConverter.ConvertTo<MetricTimeSpan>(timeDelta, tempoMap);
            await Task.Delay(TimeSpan.FromMilliseconds(delay.TotalMilliseconds));
            ProcessMidiEvent(timedEvent.Event);
        }
    }

    private void ProcessMidiEvent(MidiEvent midiEvent)
    {
        switch (midiEvent)
        {
            case NoteOnEvent noteOn:
                if (noteOn.Velocity > 0)
                {
                    // Console.WriteLine($"note on in {noteOn.GetNoteName()} on octave {noteOn.GetNoteOctave()}");
                    _synth.NoteOn(noteOn.Channel, noteOn.NoteNumber, noteOn.Velocity);
                    MidiKeyOn?.Invoke(this, new() { Octave = noteOn.GetNoteOctave(), NoteName = noteOn.GetNoteName() });
                }
                else
                {
                    _synth.NoteOff(noteOn.Channel, noteOn.NoteNumber);
                    // Console.WriteLine($"note of in {noteOn.GetNoteName()} on octave {noteOn.GetNoteOctave()}");
                    MidiKeyOff?.Invoke(this, new() { Octave = noteOn.GetNoteOctave(), NoteName = noteOn.GetNoteName() });
                }
                break;

            case NoteOffEvent noteOff:
                _synth.NoteOff(noteOff.Channel, noteOff.NoteNumber);
                // Console.WriteLine($"note of in {noteOff.GetNoteName()} on octave {noteOff.GetNoteOctave()}");
                MidiKeyOff?.Invoke(this, new() { Octave = noteOff.GetNoteOctave(), NoteName = noteOff.GetNoteName() });
                break;

            case ControlChangeEvent controlChange:
                _synth.ControlChange(controlChange.Channel, controlChange.ControlNumber, controlChange.ControlValue);
                break;

            case ProgramChangeEvent programChange:
                _synth.ProgramChange(programChange.Channel, programChange.ProgramNumber);
                break;

            case PitchBendEvent pitchBend:
                _synth.PitchBend(pitchBend.Channel, pitchBend.PitchValue);
                break;

            case ChannelAftertouchEvent aftertouch:
                _synth.ControlChange(aftertouch.Channel, 128, aftertouch.AftertouchValue); // Channel pressure
                break;
        }
    }

}



public record Note
{
    public int Octave { get; set; }
    public NoteName NoteName { get; set; }

}
