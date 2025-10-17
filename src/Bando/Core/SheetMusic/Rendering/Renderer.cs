using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Bando.Controls;
namespace Bando.Core.SheetMusic.Rendering;
public class SheetMusicRenderer
{
    private bool _disposed = false;
    private Verovio _verovio = new();
    private Sheet _sheetControl;
    private CancellationTokenSource _renderCancellationToken = new();
    private List<string> _svgs = new();
    private CancellationTokenSource? _boundsChangedCts;
    private readonly TimeSpan _debounceDelay = TimeSpan.FromMilliseconds(150);

    public SheetMusicRenderer(Sheet sheetControl)
    {
        _sheetControl = sheetControl;
        _sheetControl.PropertyChanged += SheetControlPropertyChanged;
    }

    private void SheetControlPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Sheet.BoundsProperty)
        {
            if (e.OldValue is null) return;
            var oldBounds = (Rect)e.OldValue;
            if (_sheetControl.Bounds.Width != oldBounds.Width)
            {
                foreach (var child in _sheetControl.Children)
                {
                    if (child is PageCanvas canvas)
                    {
                        var originalHeight = canvas.OriginalSvgHeight;
                        var originalWidth = canvas.OriginalSvgWidth;
                        if (originalHeight is null || originalWidth is null) return;

                        var aspectRatio = originalWidth / originalHeight;
                        var targetHeight = canvas.Bounds.Width / aspectRatio ?? 0;
                        canvas.Height = targetHeight;
                        var scaleX = canvas.Bounds.Width / originalWidth;
                        var scaleY = targetHeight / originalHeight;

                        var group = canvas.Children[0] as LogicalSvgGroup;
                        var transformGroup = (TransformGroup?)group?.RenderTransform;
                        //pop previous transform
                        transformGroup?.Children.RemoveAt(transformGroup.Children.Count - 1);
                        transformGroup?.Children.Add(new ScaleTransform(scaleX ?? 0, scaleY ?? 0));
                    }
                }
            }
        }
    }
    private Dictionary<string, (int pageIndex, NoteLogicalSvgGroup group)> _noteCache = new();
    private HashSet<string> _currentlyHighlightedNotes = new();
    public async void MidiNoteChanged(double ms)
    {
        if (_svgs.Count == 0) return;
        var noteAt = _verovio.ElementsAtTime(ms);
        if (noteAt is null) return;
        var result = JsonSerializer.Deserialize(noteAt, VerovioJsonContext.Default.VerovioElementAtTimeModel);
        if (result is null) return;
        var newNotes = result.notes.ToHashSet();
        var toUnhighlight = _currentlyHighlightedNotes.Except(newNotes).ToList();
        var toHighlight = newNotes.Except(_currentlyHighlightedNotes).ToList();
        if (toUnhighlight.Count == 0 && toHighlight.Count == 0) return;
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            foreach (var noteId in toUnhighlight)
            {
                if (_noteCache.TryGetValue(noteId, out var cached))
                {
                    cached.group.SetHighlighted(false);
                }
                _currentlyHighlightedNotes.Remove(noteId);
            }
            foreach (var noteId in toHighlight)
            {
                var noteGroup = GetOrCacheNoteGroup(noteId, result.page - 1);
                if (noteGroup != null)
                {
                    noteGroup.SetHighlighted(true);
                    _currentlyHighlightedNotes.Add(noteId);
                }
            }
        });
    }

    private NoteLogicalSvgGroup? GetOrCacheNoteGroup(string noteId, int pageIndex)
    {
        if (_noteCache.TryGetValue(noteId, out var cached))
        {
            return cached.group;
        }

        var page = _sheetControl.Children[pageIndex] as PageCanvas;
        if (page is null) return null;

        var noteGroup = page.GetVisualDescendants()
            .OfType<NoteLogicalSvgGroup>()
            .FirstOrDefault(p => p.NoteId == noteId);

        if (noteGroup != null)
        {
            _noteCache[noteId] = (pageIndex, noteGroup);
        }

        return noteGroup;
    }

    public void ClearCache()
    {
        _noteCache.Clear();
        _currentlyHighlightedNotes.Clear();
    }
    public async void InitAsync(string source)
    {
        CancelRenders();
        ClearCache();
        await Task.Run(async () =>
        {
            _svgs = new();
            _verovio.LoadData(MuseScoreManager.FromMidiToMei(source));
            var pagecount = _verovio.GetPageCount();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _sheetControl.Purge();
                _sheetControl.SetPageCount(pagecount);
            });
            for (int i = 0; i < pagecount; i++)
            {
                _svgs.Add(_verovio.RenderToSvg(i + 1));
            }
        });
        RenderAll();
    }

    public void RenderAll()
    {
        CancelRenders();
        for (int i = 0; i < _svgs.Count; i++)
        {
            var capture = i;
            _ = RenderPageAsync(capture, _renderCancellationToken.Token);
        }
    }

    public async Task RenderPageAsync(int pageIndex, CancellationToken ctx)
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (ctx.IsCancellationRequested) return;
                var renderer = new VerovioSvgRenderer();
                var dom = renderer.Load(_svgs[pageIndex]);
                var canvas = _sheetControl.Children[pageIndex] as PageCanvas;
                if (canvas is null) return;
                var aspectRatio = renderer.Width / renderer.Height;
                var targetHeight = canvas.Bounds.Width / aspectRatio;
                canvas.Height = targetHeight;

                var scaleX = canvas.Bounds.Width / renderer.Width;
                var scaleY = targetHeight / renderer.Height;

                var group = dom[0] as LogicalSvgGroup;
                if (group is null) return;
                var transformGroup = (TransformGroup?)group.RenderTransform;
                transformGroup?.Children.Add(new ScaleTransform(scaleX, scaleY));
                canvas.Children.Add(group);
                canvas.OriginalSvgHeight = renderer.Height;
                canvas.OriginalSvgWidth = renderer.Width;

            }, DispatcherPriority.Background, ctx);
        }
        catch (OperationCanceledException)
        {
            // Render was cancelled
        }
    }
    private void CancelRenders()
    {
        _renderCancellationToken.Cancel();
        _renderCancellationToken.Dispose();
        _renderCancellationToken = new();
    }
}

internal class VerovioElementAtTimeModel
{
    public List<string> chords { get; set; } = new();
    public string measure { get; set; } = "";
    public List<string> notes { get; set; } = new();
    public int page { get; set; }
    public List<string> rests { get; set; } = new();
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(VerovioElementAtTimeModel))]
internal partial class VerovioJsonContext : JsonSerializerContext
{
}
