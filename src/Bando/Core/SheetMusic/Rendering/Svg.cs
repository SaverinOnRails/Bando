namespace Bando.Core.SheetMusic.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Svg;
using Svg.Transforms;

internal class VerovioSvgRenderer
{
    private Dictionary<string, PathData> _globalDefs = new();
    private DrawingGroup _drawings = new();
    private Canvas _notes = new();
    private List<Control> _renderDom = new();
    public double Height { get; private set; }
    public double Width { get; private set; }

    public List<Control> Load(string svg)
    {
        SvgDocument svgDoc = SvgDocument.FromSvg<SvgDocument>(svg);
        Height = svgDoc.ViewBox.Height;
        Width = svgDoc.ViewBox.Width;
        foreach (SvgElement elem in svgDoc.Children)
        {
            if (elem is SvgDefinitionList defs)
            {
                HandleDefs(defs);
            }
            else if (elem is SvgDescription) { }
            else if (elem is SvgUnknownElement unknown)
            {
            }
            else if (elem is SvgFragment frag)
            {
                HandleSvgFragment(frag);
            }
            else throw new UnhandledElementException(elem);
        }
        return _renderDom;
    }

    private void HandleSvgFragment(SvgFragment frag)
    {
        var fragmentContainer = new LogicalSvgGroup();
        if (frag.ViewBox != SvgViewBox.Empty)
        {
            double scaleX = Width / frag.ViewBox.Width;
            double scaleY = Height / frag.ViewBox.Height;
            var containerTransform = new TransformGroup();
            containerTransform.Children.Add(new ScaleTransform(scaleX, scaleY));
            fragmentContainer.RenderTransform = containerTransform;
        }
        foreach (var elem in frag.Children)
        {
            if (elem is SvgGroup g)
            {
                RenderGroup(g);
            }
            else throw new UnhandledElementException(elem);
        }
        var drawingimage = new DrawingImage(_drawings);
        var image = new Image()
        {
            Source = new DrawingImage(_drawings),
            Width = frag.ViewBox.Width,
            Height = frag.ViewBox.Height,
            Stretch = Stretch.None,
        };
        _notes.Width = frag.ViewBox.Width;
        _notes.Height = frag.ViewBox.Height;
        fragmentContainer.Children.Add(_notes);
        fragmentContainer.Children.Add(image);
        _renderDom.Add(fragmentContainer);
    }
    private void RenderGroup(SvgGroup g)
    {
        foreach (var elem in g.Children)
        {
            if (elem is SvgGroup _g)
            {
                if (_g.Children.Count > 0)
                {
                    RenderGroup(_g);
                }
            }
            else if (elem is SvgPath _path)
            {
                _drawings.Children.Add(_path.ToAvaloniaDrawing());
            }
            else if (elem is SvgText _text) { } //fuck this for now
            else if (elem is SvgUse _use)
            {
                var dictEnry = _use.ReferencedElement.OriginalString.TrimStart('#');
                if (_globalDefs.TryGetValue(dictEnry, out var path))
                {
                    TransformGroup totalTransformGroup = new();
                    var p = path.ToAvaloniaControl();
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
                    p.RenderTransform = totalTransformGroup;
                    _notes.Children.Add(p);
                }
            }
            else if (elem is SvgPolygon _polygon)
            {
                _drawings.Children.Add(_polygon.ToAvaloniaDrawing());
            }
            else if (elem is SvgPolyline _polyline)
            {
                _drawings.Children.Add(_polyline.ToAvaloniaDrawing());
            }
            else if (elem is SvgEllipse _ellipse)
            {
                _drawings.Children.Add(_ellipse.ToAvaloniaDrawing());
            }
            else if (elem is SvgRectangle _rect)
            {
                _drawings.Children.Add(_rect.ToAvaloniaDrawing());
            }
            else throw new UnhandledElementException(elem);
        }
    }

    private void HandleDefs(SvgDefinitionList defs)
    {
        foreach (var elem in defs.Children)
        {
            if (elem is SvgGroup g)
            {
                var groupId = g.ID;
                foreach (var path in g.Children)
                {
                    if (path is SvgPath p)
                    {
                        _globalDefs[groupId] = new()
                        {
                            Data = p.PathData.ToString(),
                            Transform = p.ToAvaloniaTransformGroup(),
                        };
                    }
                    else throw new UnhandledElementException(elem);
                }
            }
            else throw new UnhandledElementException(elem);
        }
    }

}

public interface ISvgRenderEception
{

}
public class UnhandledElementException : Exception, ISvgRenderEception
{
    public UnhandledElementException(SvgElement v) : base($"Unhandled Element : {v.GetType().ToString()}")
    {

    }
}
public class UnhandledTransformException : Exception, ISvgRenderEception
{
    public UnhandledTransformException(SvgTransform v) : base($"Unhandled Element : {v.GetType().ToString()}")
    {

    }
}

public class LogicalSvgGroup : Canvas
{
    public List<string> Class { get; set; } = new();
    public string? Id { get; set; }
}

[PseudoClasses(":highlighted")]
public class NoteLogicalSvgGroup : LogicalSvgGroup
{
    public string? NoteId { get; set; }

    public void SetHighlighted(bool highlighted)
    {
        PseudoClasses.Set(":highlighted", highlighted);
    }
}

internal record PathData
{
    public required string Data { get; set; }
    public required Transform Transform { get; set; }
    public Path ToAvaloniaControl()
    {
        var path = new Avalonia.Controls.Shapes.Path();
        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(Transform);
        string pathData = Data ?? string.Empty;
        var geometry = Geometry.Parse(pathData);
        path.Stretch = Stretch.None;
        path.Data = geometry;
        path.StrokeLineCap = PenLineCap.Flat;
        path.StrokeJoin = 0;
        path.StrokeThickness = 8;
        return path;
    }
}

public static class SvgExtensions
{
    public static List<string> GetClasses(this SvgElement element)
    {
        if (element.CustomAttributes.TryGetValue("class", out var cls))
            return cls.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        return new List<string>();
    }

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
                else throw new UnhandledTransformException(transform);
            }
        }
        return transformGroup;
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
        return path;
    }

    public static Avalonia.Media.GeometryDrawing ToAvaloniaDrawing(this SvgPath self)
    {
        string pathData = self.PathData?.ToString() ?? string.Empty;
        var geometry = Geometry.Parse(pathData);
        var drawing = new GeometryDrawing
        {
            Geometry = geometry,
            Brush = Brushes.Black,
            Pen = new Pen
            {
                Brush = Brushes.Black,
                Thickness = (double)self.StrokeWidth.Value,
                LineCap = self.StrokeLineCap.ToAvaloniaPenLineCap(),
                LineJoin = self.StrokeLineJoin.ToAvaloniaPenLineJoin()
            }
        };

        return drawing;
    }
    public static Avalonia.Controls.Shapes.Path DeepCopy(this Path self)
    {
        return new()
        {
            Data = self.Data?.Clone(),
            // Stroke = self.Stroke,
            // Fill = self.Fill,
            StrokeThickness = self.StrokeThickness,
            StrokeLineCap = self.StrokeLineCap,
            StrokeJoin = self.StrokeJoin,
            Stretch = self.Stretch
        };
    }

    public static Avalonia.Media.GeometryDrawing ToAvaloniaDrawing(this SvgEllipse self)
    {
        var ellipseGeometry = new EllipseGeometry(
            new Rect(
                self.CenterX - self.RadiusX,
                self.CenterY - self.RadiusY,
                self.RadiusX * 2,
                self.RadiusY * 2
            )
        );
        var drawing = new GeometryDrawing
        {
            Geometry = ellipseGeometry,
            Brush = Brushes.Black,
            Pen = new Pen
            {
                Brush = Brushes.Black,
                Thickness = 1.0
            }
        };
        return drawing;
    }
    public static Avalonia.Media.GeometryDrawing ToAvaloniaDrawing(this SvgPolygon self)
    {
        var points = new List<Point>();
        for (int i = 0; i < self.Points.Count - 1; i += 2)
        {
            double x = (double)self.Points[i].Value;
            double y = (double)self.Points[i + 1].Value;
            points.Add(new Point(x, y));
        }

        var polylineGeometry = new PolylineGeometry(points, true);
        var drawing = new GeometryDrawing
        {
            Geometry = polylineGeometry,
            Brush = Brushes.Black,
            Pen = new Pen
            {
                Brush = Brushes.Black,
                Thickness = 1.0
            }
        };

        return drawing;
    }
    public static Avalonia.Media.GeometryDrawing ToAvaloniaDrawing(this SvgPolyline self)
    {
        var points = new List<Point>();
        for (int i = 0; i < self.Points.Count - 1; i += 2)
        {
            double x = (double)self.Points[i].Value;
            double y = (double)self.Points[i + 1].Value;
            points.Add(new Point(x, y));
        }
        var polylineGeometry = new PolylineGeometry(points, false);
        var drawing = new GeometryDrawing
        {
            Geometry = polylineGeometry,
            Brush = Brushes.Transparent,
            Pen = new Pen
            {
                Brush = new SolidColorBrush(Colors.Black, self.StrokeOpacity),
                Thickness = (double)self.StrokeWidth,
                LineCap = self.StrokeLineCap.ToAvaloniaPenLineCap(),
                LineJoin = self.StrokeLineJoin.ToAvaloniaPenLineJoin()
            }
        };

        return drawing;
    }
    public static Avalonia.Media.GeometryDrawing ToAvaloniaDrawing(this SvgRectangle self)
    {
        var rectGeometry = new RectangleGeometry(
            new Rect(
                (double)self.X.Value,
                (double)self.Y.Value,
                (double)self.Width.Value,
                (double)self.Height.Value
            ),
            (double)self.CornerRadiusX.Value,
            (double)self.CornerRadiusY.Value
        );
        var drawing = new GeometryDrawing
        {
            Geometry = rectGeometry,
            Brush = Brushes.Transparent,
            Pen = new Pen
            {
                Brush = new SolidColorBrush(Colors.Black, self.StrokeOpacity),
                Thickness = (double)self.StrokeWidth,
            }
        };
        return drawing;
    }
}
