namespace Bando.Core.Midi;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public class MidiPlayer : IDisposable
{
    private MidiFile? _midiFile = null;
    private List<TimedEvent>? _timedEvents = null;
    private FluidSynth _synth = new();
    private Task? _playbackTask = null;

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void LoadMidiFile(string file)
    {
        _midiFile = MidiFile.Read(file);
        _timedEvents = _midiFile.GetTimedEvents().ToList();
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
                    _synth.NoteOn(noteOn.Channel, noteOn.NoteNumber, noteOn.Velocity);
                    Console.WriteLine($"Note On: {noteOn.NoteNumber}, Velocity: {noteOn.Velocity}, Channel: {noteOn.Channel}");
                }
                else
                {
                    _synth.NoteOff(noteOn.Channel, noteOn.NoteNumber);
                    Console.WriteLine($"Note Off: {noteOn.NoteNumber}, Channel: {noteOn.Channel}");
                }
                break;

            case NoteOffEvent noteOff:
                _synth.NoteOff(noteOff.Channel, noteOff.NoteNumber);
                Console.WriteLine($"Note Off: {noteOff.NoteNumber}, Channel: {noteOff.Channel}");
                break;

            case ControlChangeEvent controlChange:
                _synth.ControlChange(controlChange.Channel, controlChange.ControlNumber, controlChange.ControlValue);
                Console.WriteLine($"Control Change: Controller {controlChange.ControlNumber}, Value: {controlChange.ControlValue}, Channel: {controlChange.Channel}");
                break;

            case ProgramChangeEvent programChange:
                _synth.ProgramChange(programChange.Channel, programChange.ProgramNumber);
                Console.WriteLine($"Program Change: {programChange.ProgramNumber} on Channel {programChange.Channel}");
                break;

            case PitchBendEvent pitchBend:
                _synth.PitchBend(pitchBend.Channel, pitchBend.PitchValue);
                Console.WriteLine($"Pitch Bend: {pitchBend.PitchValue} on Channel {pitchBend.Channel}");
                break;

            case ChannelAftertouchEvent aftertouch:
                _synth.ControlChange(aftertouch.Channel, 128, aftertouch.AftertouchValue); // Channel pressure
                Console.WriteLine($"Channel Aftertouch: {aftertouch.AftertouchValue} on Channel {aftertouch.Channel}");
                break;
        }
    }
}

