namespace Bando.Core.Midi;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public class MidiPlayer
{
    private MidiFile? _midiFile = null;
    private List<TimedEvent>? _timedEvents = null;
    private Task? _playbackTask = null;

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
                    Console.WriteLine($"Note On: {noteOn.NoteNumber}, Velocity: {noteOn.Velocity}, Channel: {noteOn.Channel}");
                }
                else
                {
                    Console.WriteLine($"Note Off: {noteOn.NoteNumber}, Channel: {noteOn.Channel}");
                }
                break;

            case NoteOffEvent noteOff:
                Console.WriteLine($"Note Off: {noteOff.NoteNumber}, Channel: {noteOff.Channel}");
                break;

            case ControlChangeEvent controlChange:
                Console.WriteLine($"Control Change: Controller {controlChange.ControlNumber}, Value: {controlChange.ControlValue}");
                break;

            case ProgramChangeEvent programChange:
                Console.WriteLine($"Program Change: {programChange.ProgramNumber} on Channel {programChange.Channel}");
                break;

        }
    }
}

public static class MetricTimeSpanExtensions
{
    public static TimeSpan ToTimeSpan(this MetricTimeSpan metricTimeSpan)
    {
        return new(metricTimeSpan.Hours, metricTimeSpan.Minutes, metricTimeSpan.Seconds);
    }
}
