using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
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
        _renderOptions = new ResvgOptions();
        _renderOptions.LoadSystemFonts();
        _renderTree = new(_renderOptions);
        _sheetControl = sheetControl;
        _sheetControl.PropertyChanged += SheetControlPropertyChanged;
    }

    private void SheetControlPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Sheet.BoundsProperty)
        {
            if (e.OldValue is null) return;
            var oldBounds = (Rect)e.OldValue;
            if (_sheetControl.Bounds.Width > oldBounds.Width)
            {
                _boundsChangedCts?.Cancel();
                _boundsChangedCts?.Dispose();
                _boundsChangedCts = new();
                var token = _boundsChangedCts.Token;
                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(_debounceDelay, token);
                        RenderAll();
                    }
                    catch { }
                });
            }
        }
    }

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

    public async void MidiNoteChanged(double ms)
    {
        return;
        if (_svgs.Count == 0) return;
        var noteAt = _verovio.ElementsAtTime(ms);
        if (noteAt is null) return;
        var result = JsonSerializer.Deserialize(noteAt, VerovioJsonContext.Default.VerovioElementAtTimeModel);
        if (result is null) return;
        var svg = _svgs[result.page - 1];

        var svgDoc = XDocument.Parse(svg);
        XNamespace ns = SvgNamespace;
        foreach (var noteId in result.notes)
        {
            await HighlightElement(svgDoc, ns, noteId, result.page - 1);
        }
    }

    private async Task HighlightElement(XDocument svgDoc, XNamespace ns, string noteId, int pageIndex)
    {
        var element = svgDoc.Descendants(ns + "g").FirstOrDefault(e => e.Attribute("id")?.Value == noteId);
        if (element is null) return;
        var styleAttr = element.Attribute("style");
        if (styleAttr != null)
        {
            styleAttr.Value += "; fill: #ff0000; opacity: 0.7;";
        }
        else
        {
            element.Add(new XAttribute("style", "fill: #ff0000; opacity: 0.7;"));
        }
        var svg = svgDoc.ToString();
        // CancelRenders();
        await RenderPageAsync(pageIndex, _renderCancellationToken.Token, svg);
    }

    private readonly SemaphoreSlim _renderLock = new SemaphoreSlim(1, 1);
    public async Task RenderPageAsync(int pageIndex, CancellationToken ctx, string? rawSvg = null)
    {
        try
        {
            ctx.ThrowIfCancellationRequested();
            if (_renderTree is null || _svgs is null) return;
            var (image, pwidth, pheight) = await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var img = _sheetControl.Children[pageIndex] as Image;
                var parentPanel = img?.FindAncestorOfType<Panel>();
                return (img, parentPanel?.Bounds.Width, parentPanel?.Bounds.Height);
            });
            if (image is null || pheight is null || pwidth is null) return;
            ctx.ThrowIfCancellationRequested();
            var writableBitmap = await Task.Run(async () =>
            {
                await _renderLock.WaitAsync(ctx);
                _renderTree.ParseFromString(rawSvg == null ? _svgs[pageIndex] : rawSvg);
                var imageSize = _renderTree.GetImageSize();

                float scale = (float)pwidth / imageSize.width;
                int width = (int)pwidth;
                int height = (int)(imageSize.height * scale);
                var pixmap = new byte[width * height * 4];

                var transform = ResvgTransform.Identity();
                transform.a = scale;
                transform.d = scale;
                _renderTree.Render(transform, (uint)width, (uint)height, pixmap);
                WriteableBitmap? bitmap;
                // lookup cache for suitable bitmap
                if (!_bitmapCache.TryGetValue(pageIndex, out bitmap))
                {
                    ctx.ThrowIfCancellationRequested();
                    bitmap = new WriteableBitmap(
                        new PixelSize(width, height),
                        new Vector(96, 96),
                        PixelFormat.Rgba8888,
                        AlphaFormat.Premul
                    );
                    _bitmapCache.TryAdd(pageIndex, bitmap);
                }
                else
                {
                    if (bitmap.PixelSize.Height != height || bitmap.PixelSize.Width != width)
                    {
                        ctx.ThrowIfCancellationRequested();
                        bitmap.Dispose();
                        bitmap = new WriteableBitmap(
                            new PixelSize(width, height),
                            new Vector(96, 96),
                            PixelFormat.Rgba8888,
                            AlphaFormat.Premul
                        );
                        _bitmapCache[pageIndex] = bitmap;
                    }
                }
                using (var lockedFramebuffer = bitmap.Lock())
                {
                    Console.WriteLine("Copying new bitmap");
                    Marshal.Copy(pixmap, 0, lockedFramebuffer.Address, pixmap.Length);
                }
                return bitmap;
            });
            ctx.ThrowIfCancellationRequested();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                image.Source = writableBitmap;
            });
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was cancelled");
        }
        catch { }
        finally
        {
            _renderLock.Release();
        }
    }
    public void RenderAll()
    {
        CancelRenders();
        for (int i = 0; i < _svgs.Count; i++)
        {
            var capture = i;
            Task.Run(() => RenderPageAsync(capture, _renderCancellationToken.Token), _renderCancellationToken.Token);
        }
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
