using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Svg;
using Svg.Transforms;

namespace Bando.Core.SheetMusic.Rendering;

internal class VerovioSvgRenderer
{
    private SvgDocument _document;
    private Dictionary<string, PathDefinitionData> _pathDefinitions = new();
    private Canvas _mainCanvas = new();
    private Canvas _notes = new();
    // private DrawingGroup _drawings = new();
    public double Height { get; private set; }
    public double Width { get; private set; }
    public VerovioSvgRenderer(string svg)
    {
        _document = SvgDocument.FromSvg<SvgDocument>(svg);
        Load();
    }

    private void Load()
    {
        foreach (var elem in _document.Children)
        {
            switch (elem)
            {
                case SvgDefinitionList definitionList:
                    HandleDefinitionList(definitionList);
                    break;
                case SvgFragment fragment:
                    HandleMainSvg(fragment);
                    break;
            }
        }
    }

    private void HandleMainSvg(SvgFragment fragment)
    {
        var viewbox = fragment.ViewBox;
        _mainCanvas.Height = viewbox.Height;
        _mainCanvas.Width = viewbox.Width;
        _notes.Height = viewbox.Height;
        _notes.Width = viewbox.Width;
        _notes.RenderTransform = null;
        foreach (var elem in fragment.Children)
        {
            if (elem is SvgGroup group)
            {
                HandleGroup(group, null);
            }
        }
        // var image = new Image()
        // {
        //     Source = new DrawingImage(_drawings),
        //     Stretch = Stretch.None,
        //     Height = viewbox.Height,
        //     Width = viewbox.Width,
        // };
        // Canvas u = new();
        // u.Children.Add(image);
        // _mainCanvas.Children.Add(u);
        _mainCanvas.Children.Add(_notes);
    }

    private void HandleGroup(SvgGroup group, string? noteid)
    {
        foreach (var elem in group.Children)
        {
            switch (elem)
            {
                case SvgGroup _g:
                    var maybe_noteid = noteid ?? (_g.GetClasses().Contains("note") ? _g.ID : null);
                    HandleGroup(_g, maybe_noteid);
                    break;
                case SvgPath _path:
                    _notes.Children.Add(_path.ToAvaloniaPath());
                    // _drawings.Children.Add(_path.ToAvaloniaDrawing());
                    break;
                case SvgUse _use:
                    var dictEnry = _use.ReferencedElement.OriginalString.TrimStart('#');
                    if (_pathDefinitions.TryGetValue(dictEnry, out var path))
                    {
                        Canvas usegroup = new();
                        TransformGroup totalTransformGroup = new();
                        var p = path.ToAvaloniaControl();
                        if (noteid is not null) p.NoteId = noteid;
                        var originalPathTransform = path.Transform;
                        //apply the original transformation first
                        if (originalPathTransform is not null)
                        {
                            totalTransformGroup.Children.Add((Transform)originalPathTransform);
                        }
                        foreach (var t in _use.ToAvaloniaTransformGroup().Children)
                        {
                            totalTransformGroup.Children.Add(t);
                        }
                        usegroup.RenderTransform = totalTransformGroup;
                        usegroup.Children.Add(p);
                        _notes.Children.Add(usegroup);
                    }
                    break;
            }
        }
    }

    public Canvas Content => _mainCanvas;
    private void HandleDefinitionList(SvgDefinitionList definitionList)
    {
        foreach (var element in definitionList.Children)
        {
            if (element is SvgGroup group)
            {
                var id = group.ID;
                foreach (var path in group.Children)
                {
                    if (path is SvgPath p)
                    {
                        _pathDefinitions.Add(id, new()
                        {
                            PathData = p.PathData.ToString(),
                            Transform = p.ToAvaloniaTransformGroup()
                        });
                    }
                }
            }
        }
    }
}

[PseudoClasses(":highlighted")]
internal class NotePath : Path
{
    public string NoteId { get; set; } = "";
    public void SetHighlighted(bool highlighted)
    {
        PseudoClasses.Set(":highlighted", highlighted);
    }
}
internal record PathDefinitionData
{
    public required string PathData { get; set; }
    public required Transform Transform { get; set; }
    public NotePath ToAvaloniaControl()
    {
        var path = new NotePath();
        // var transformGroup = new TransformGroup();
        // transformGroup.Children.Add(Transform);
        string pathData = PathData ?? string.Empty;
        var geometry = Geometry.Parse(pathData);
        path.Stretch = Stretch.None;
        path.Data = geometry;
        path.StrokeLineCap = PenLineCap.Flat;
        path.StrokeJoin = 0;
        path.StrokeThickness = 8;
        return path;
    }
}

internal static class SvgExtensions
{
    public static TransformGroup ToAvaloniaTransformGroup(this SvgElement self)
    {
        var transformGroup = new TransformGroup();
        if (self.Transforms is not null)
        {
            //create transform group in reverse order because svgs are retarded
            for (int i = self.Transforms.Count - 1; i >= 0; i--)
            {
                var transform = self.Transforms[i];
                if (transform is SvgTranslate t)
                {
                    transformGroup.Children.Add(new TranslateTransform(t.X, t.Y));
                }
                else if (transform is SvgScale s)
                {
                    transformGroup.Children.Add(new ScaleTransform(s.X, s.Y));
                }
            }
        }
        return transformGroup;
    }

    public static Avalonia.Media.GeometryDrawing ToAvaloniaDrawing(this SvgPath self)
    {
        string pathData = self.PathData?.ToString() ?? string.Empty;
        var geometry = Geometry.Parse(pathData);
        var drawing = new GeometryDrawing
        {
            Geometry = geometry,
            Brush = new SolidColorBrush(Color.Parse("#e5e5e5")),
            Pen = new Pen
            {
                Brush = new SolidColorBrush(Color.Parse("#e5e5e5")),
                Thickness = (double)self.StrokeWidth.Value,
                LineCap = self.StrokeLineCap.ToAvaloniaPenLineCap(),
                LineJoin = self.StrokeLineJoin.ToAvaloniaPenLineJoin()
            }
        };
        return drawing;
    }

    public static PenLineJoin ToAvaloniaPenLineJoin(this SvgStrokeLineJoin self)
    {
        return self switch
        {
            SvgStrokeLineJoin.Miter => PenLineJoin.Miter,
            SvgStrokeLineJoin.Round => PenLineJoin.Round,
            SvgStrokeLineJoin.Bevel => PenLineJoin.Bevel,
            _ => PenLineJoin.Miter
        };
    }

    public static PenLineCap ToAvaloniaPenLineCap(this SvgStrokeLineCap self)
    {
        return self switch
        {
            SvgStrokeLineCap.Butt => PenLineCap.Flat,
            SvgStrokeLineCap.Round => PenLineCap.Round,
            SvgStrokeLineCap.Square => PenLineCap.Square,
            _ => PenLineCap.Flat
        };
    }

    public static Avalonia.Controls.Shapes.Path ToAvaloniaPath(this SvgPath self)
    {
        var path = new Avalonia.Controls.Shapes.Path();
        path.RenderTransform = self.ToAvaloniaTransformGroup();
        string pathData = self.PathData?.ToString() ?? string.Empty;
        var geometry = Geometry.Parse(pathData);
        path.Stretch = Stretch.None;
        path.Data = geometry;
        path.StrokeLineCap = self.StrokeLineCap.ToAvaloniaPenLineCap();
        path.StrokeJoin = self.StrokeLineJoin.ToAvaloniaPenLineJoin();
        path.StrokeThickness = (double)self.StrokeWidth.Value;
        path.Fill = new SolidColorBrush(Color.Parse("#e5e5e5"));
        path.Stroke = new SolidColorBrush(Color.Parse("#e5e5e5"));
        return path;
    }

    public static List<string> GetClasses(this SvgElement element)
    {
        if (element.CustomAttributes.TryGetValue("class", out var cls))
            return cls.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        return new List<string>();
    }
}

