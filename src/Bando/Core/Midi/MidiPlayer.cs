namespace Bando.Core.Midi;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;

public delegate void MidiKeyEventHandler(object sender, Note e);
public delegate void MidiPlaybackLocationChangedHandler(object sender, double newLocation);

public class MidiPlayer : IDisposable
{
    private MidiFile? _midiFile = null;
    private List<TimedEvent>? _timedEvents = null;
    private FluidSynth _synth = new();
    private Task? _playbackTask = null;
    private bool _disposed = false;
    private CancellationTokenSource _playbackTaskCancellationToken = new();

    private bool _paused = false;
    public event MidiKeyEventHandler? MidiKeyOn;
    public event MidiKeyEventHandler? MidiKeyOff;
    public event MidiPlaybackLocationChangedHandler? MidiPlaybackLocationChanged;
    public event EventHandler? TurnOffAllNotes;
    public double PlaybackDuration { get; private set; } = 0;
    public double PlaybackLocation { get; private set; } = 0;
    private Stopwatch _playbackStopwatch = new();
    private ManualResetEvent _pauseEvent = new(true);

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
        PlaybackDuration = _midiFile.GetDuration<MetricTimeSpan>().TotalMilliseconds;
    }
    public void StartPlayback()
    {
        _playbackTask = Task.Run(() => PlaybackTask(_playbackTaskCancellationToken.Token, 0));
    }
    private async Task PlaybackTask(CancellationToken ctx, double startPos)
    {
        try
        {
            if (_timedEvents is null || _midiFile is null) return;
            var tempoMap = _midiFile.GetTempoMap();
            _playbackStopwatch.Restart();
            ctx.ThrowIfCancellationRequested();
            int startIndex = GetIndexFromPosition(startPos);
            for (int index = startIndex; index < _timedEvents.Count(); index++)
            {
                var timedEvent = _timedEvents[index];
                var targetTime = TimeConverter.ConvertTo<MetricTimeSpan>(timedEvent.Time, tempoMap);
                double targetMillisecs = targetTime.TotalMilliseconds - startPos;
                ctx.ThrowIfCancellationRequested();
                while (true)
                {
                    _pauseEvent.WaitOne();
                    double currentTime = _playbackStopwatch.Elapsed.TotalMilliseconds;
                    double remaining = targetMillisecs - currentTime;
                    if (remaining <= 0)
                        break;
                    PlaybackLocation = currentTime + startPos;
                    MidiPlaybackLocationChanged?.Invoke(this, PlaybackLocation);
                    ctx.ThrowIfCancellationRequested();
                    await Task.Delay(Math.Min((int)remaining, 10));
                }
                ctx.ThrowIfCancellationRequested();
                _pauseEvent.WaitOne();
                ProcessMidiEvent(timedEvent.Event);
            }
            PlaybackLocation = PlaybackDuration;
            MidiPlaybackLocationChanged?.Invoke(this, PlaybackLocation);
        }
        catch
        {
        }
    }

    private int GetIndexFromPosition(double pos)
    {
        if (_midiFile is null || _timedEvents is null) return 0;
        var tempoMap = _midiFile.GetTempoMap();
        for (int i = 0; i < _timedEvents.Count; i++)
        {
            var eventTime = TimeConverter.ConvertTo<MetricTimeSpan>(_timedEvents[i].Time, tempoMap);
            if (eventTime.TotalMilliseconds >= pos)
                return i;
        }

        return _timedEvents.Count;
    }
    private void ProcessMidiEvent(MidiEvent midiEvent)
    {
        switch (midiEvent)
        {
            case NoteOnEvent noteOn:
                if (noteOn.Velocity > 0)
                {
                    _synth.NoteOn(noteOn.Channel, noteOn.NoteNumber, noteOn.Velocity);
                    MidiKeyOn?.Invoke(this, new() { Octave = noteOn.GetNoteOctave(), NoteName = noteOn.GetNoteName() });
                }
                else
                {
                    _synth.NoteOff(noteOn.Channel, noteOn.NoteNumber);
                    MidiKeyOff?.Invoke(this, new() { Octave = noteOn.GetNoteOctave(), NoteName = noteOn.GetNoteName() });
                }
                break;
            case NoteOffEvent noteOff:
                _synth.NoteOff(noteOff.Channel, noteOff.NoteNumber);
                MidiKeyOff?.Invoke(this, new() { Octave = noteOff.GetNoteOctave(), NoteName = noteOff.GetNoteName() });
                break;
        }
    }

    public void SynthPlayNote(Note note)
    {
        var notenumber = Melanchall.DryWetMidi.MusicTheory.Note.Get(note.NoteName, note.Octave).NoteNumber;
        FourBitNumber channel = (FourBitNumber)0;
        _synth.NoteOn(channel, notenumber, 100);
    }

    public void Pause()
    {
        if (_paused == true) return;
        _paused = true;
        _playbackStopwatch.Stop();
        _pauseEvent.Reset();
    }
    public void Play()
    {
        if (_paused == false) return;
        _paused = false;
        _playbackStopwatch.Start();
        _pauseEvent.Set();
    }
    public void SeekTo(double ms)
    {
        if (_playbackTask is null) return;
        if (!_playbackTask.IsCompleted)
        {
            _playbackTaskCancellationToken.Cancel();
            _playbackTaskCancellationToken.Dispose();
        }
        PlaybackLocation = ms;
        ResetSynth();
        _playbackTaskCancellationToken = new();
        _playbackTask = Task.Run(() => PlaybackTask(_playbackTaskCancellationToken.Token, ms));
    }

    private void ResetSynth()
    {
        for (int ch = 0; ch < 16; ch++)
        {
            _synth.AllNotesOff(ch);
        }
        TurnOffAllNotes?.Invoke(this, new());
    }
}

public record Note
{
    public int Octave { get; set; }
    public NoteName NoteName { get; set; }

}
