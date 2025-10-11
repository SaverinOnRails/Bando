using System;
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
    private List<string> _svgs = new();

    public SheetMusicRenderer(Sheet sheetControl)
    {
        _renderOptions = new ResvgOptions();
        _renderOptions.LoadSystemFonts();
        _renderTree = new(_renderOptions);
        _sheetControl = sheetControl;
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
        _verovio.LoadFile("/home/noble/Midis/rev.musicxml");
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

    public async Task RenderPageAsync(int pageIndex)
    {
        if (_renderTree is null || _svgs is null) return;

        var (image, pwidth, pheight) = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var img = _sheetControl.Children[pageIndex] as Image;
            var parentPanel = img?.FindAncestorOfType<Panel>();
            return (img, parentPanel?.Bounds.Width, parentPanel?.Bounds.Height);
        });

        if (image is null || pheight is null || pwidth is null) return;

        var writableBitmap = await Task.Run(async () =>
        {
            await _renderLock.WaitAsync();
            try
            {
                _renderTree.ParseFromString(_svgs[pageIndex]);
                var imageSize = _renderTree.GetImageSize();

                float scale = (float)pwidth / imageSize.width;
                int width = (int)pwidth;
                int height = (int)(imageSize.height * scale);
                var pixmap = new byte[width * height * 4];

                var transform = LibResvg.resvg_transform_identity();
                transform.a = scale;
                transform.d = scale;
                _renderTree.Render(transform, (uint)width, (uint)height, pixmap);

                var bitmap = new WriteableBitmap(
                    new PixelSize(width, height),
                    new Vector(96, 96),
                    PixelFormat.Rgba8888,
                    AlphaFormat.Premul
                );
                using (var lockedFramebuffer = bitmap.Lock())
                {
                    Marshal.Copy(pixmap, 0, lockedFramebuffer.Address, pixmap.Length);
                }
                return bitmap;
            }
            finally
            {
                _renderLock.Release();
            }
        });
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var old = image.Source as IDisposable;
            image.Source = writableBitmap;
            old?.Dispose();
        });
    }
    public void RenderAll()
    {
        for (int i = 0; i < _svgs.Count; i++)
        {
            var capture = i;
            Task.Run(() => RenderPageAsync(capture));
        }
    }

    ~SheetMusicRenderer()
    {
        Dispose();
    }

}
