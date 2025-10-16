using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Bando.Controls;
namespace Bando.Core.SheetMusic.Rendering;
public class SheetMusicRenderer : IDisposable
{
    private bool _disposed = false;
    private ResvgOptions? _renderOptions;
    private ResvgRenderTree? _renderTree;
    private Verovio _verovio = new();
    private Sheet _sheetControl;
    private ConcurrentDictionary<int, WriteableBitmap> _bitmapCache = new();
    private CancellationTokenSource _renderCancellationToken = new();
    private List<string> _svgs = new();
    private CancellationTokenSource? _boundsChangedCts;
    private const string SvgNamespace = "http://www.w3.org/2000/svg";
    private readonly TimeSpan _debounceDelay = TimeSpan.FromMilliseconds(150);

    public SheetMusicRenderer(Sheet sheetControl)
    {
        _sheetControl = sheetControl;
    }

    // private void SheetControlPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    // {
    //     if (e.Property == Sheet.BoundsProperty)
    //     {
    //         if (e.OldValue is null) return;
    //         var oldBounds = (Rect)e.OldValue;
    //         if (_sheetControl.Bounds.Width > oldBounds.Width)
    //         {
    //             _boundsChangedCts?.Cancel();
    //             _boundsChangedCts?.Dispose();
    //             _boundsChangedCts = new();
    //             var token = _boundsChangedCts.Token;
    //             Task.Run(async () =>
    //             {
    //                 try
    //                 {
    //                     await Task.Delay(_debounceDelay, token);
    //                     await RenderAll();
    //                 }
    //                 catch { }
    //             });
    //         }
    //     }
    // }

    public void Dispose()
    {
        if (!_disposed)
        {
            _renderTree?.Dispose();
            _renderOptions?.Dispose();
            _renderTree = null;
            _renderOptions = null;
            _disposed = true;
        }
        GC.SuppressFinalize(this);
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
                _bitmapCache.Clear();
                _sheetControl.SetPageCount(pagecount);
            });
            for (int i = 0; i < pagecount; i++)
            {
                _svgs.Add(_verovio.RenderToSvg(i + 1));
            }
        });
        RenderAll();
    }

    private readonly SemaphoreSlim _renderLock = new SemaphoreSlim(1, 1);
    public void RenderPageAsync(int pageIndex, CancellationToken ctx)
    {
        Console.WriteLine("starting svg render, who is calling this methof");
        var renderer = new VerovioSvgRenderer();
        var dom = renderer.Load(_svgs[pageIndex]);
        var canvas = (_sheetControl.Children[pageIndex] as Canvas);
        if(canvas is null) return;
        canvas.Height = renderer.Height;
        canvas.Width = renderer.Width;
        foreach (var d in dom)
        {
            canvas?.Children.Add(d);
        }
    }

    public void RenderAll()
    {
        CancelRenders();
        for (int i = 0; i < _svgs.Count; i++)
        {
            var capture = i;
            RenderPageAsync(i, _renderCancellationToken.Token);
        }
        Console.WriteLine("render for all pages complete");
    }

    private void CancelRenders()
    {
        _renderCancellationToken.Cancel();
        _renderCancellationToken.Dispose();
        _renderCancellationToken = new();
    }
    ~SheetMusicRenderer()
    {
        Dispose();
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
