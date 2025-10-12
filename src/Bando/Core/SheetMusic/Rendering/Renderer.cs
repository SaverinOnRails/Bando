using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
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
                    catch
                    {
                    }
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

    public void Init()
    {
        _svgs = new();
        _verovio.LoadFile("/home/noble/Downloads/Schubert_Staendchen_D.923.mei");
        var pagecount = _verovio.GetPageCount();
        Console.WriteLine($"discovered {pagecount} pages");
        _sheetControl.Purge();
        _sheetControl.SetPageCount(pagecount);
        for (int i = 0; i < pagecount; i++)
        {
            _svgs.Add(_verovio.RenderToSvg(i + 1));
        }
    }
    private readonly SemaphoreSlim _renderLock = new SemaphoreSlim(1, 1);
    public async Task RenderPageAsync(int pageIndex, CancellationToken ctx)
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
                _renderTree.ParseFromString(_svgs[pageIndex]);
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
                        Console.WriteLine($"Disposed bitmap for page {pageIndex}");
                        bitmap = new WriteableBitmap(
                            new PixelSize(width, height),
                            new Vector(96, 96),
                            PixelFormat.Rgba8888,
                            AlphaFormat.Premul
                        );
                        Console.WriteLine($"Restored bitmap for page {pageIndex}");
                        _bitmapCache[pageIndex] = bitmap;
                    }
                }
                using (var lockedFramebuffer = bitmap.Lock())
                {
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
        _renderCancellationToken.Cancel();
        _renderCancellationToken.Dispose();

        _renderCancellationToken = new();
        for (int i = 0; i < _svgs.Count; i++)
        {
            var capture = i;
            Task.Run(() => RenderPageAsync(capture, _renderCancellationToken.Token), _renderCancellationToken.Token);
        }
    }

    ~SheetMusicRenderer()
    {
        Dispose();
    }
}
