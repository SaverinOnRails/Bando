using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
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
    private CancellationTokenSource? _boundsChangedCts;
    private const string SvgNamespace = "http://www.w3.org/2000/svg";
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

    public void RenderPageAsync(int pageIndex, CancellationToken ctx)
    {
        var renderer = new VerovioSvgRenderer();
        var dom = renderer.Load(_svgs[pageIndex]);
        var canvas = (_sheetControl.Children[pageIndex] as PageCanvas);
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
        canvas?.Children.Add(group);

        canvas!.OriginalSvgHeight = renderer.Height;
        canvas!.OriginalSvgWidth = renderer.Width;
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
