using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Bando.Controls;
namespace Bando.Core.SheetMusic.Rendering;
public class SheetMusicRenderer
{
    private bool _disposed = false;
    private Verovio _verovio = new();
    private Sheet _sheetControl;
    private CancellationTokenSource _renderCancellationToken = new();
    private List<string> _svgs = new();
    public SheetMusicRenderer(Sheet sheetControl)
    {
        _sheetControl = sheetControl;
    }

    public async void MidiNoteChanged(double ms)
    {
    }


    public async void InitAsync(string source)
    {
        CancelRenders();
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
            _ = RenderPageAsync(capture, _renderCancellationToken.Token); //only render first two pages initially
        }
    }

    public async Task RenderPageAsync(int pageIndex, CancellationToken ctx)
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (ctx.IsCancellationRequested) return;
                var renderer = new VerovioSvgRenderer(_svgs[pageIndex]);
                var viewbox = _sheetControl.Children[pageIndex] as PageCanvas;
                viewbox!.Child = renderer.Content;

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
