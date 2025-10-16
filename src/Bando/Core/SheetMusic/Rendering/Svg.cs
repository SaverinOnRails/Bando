// Attempt at a very basic svg renderer, good enough for verovio(i hope) and nothing else

namespace Bando.Core.SheetMusic.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Svg;
using Svg.Transforms;

internal class VerovioSvgRenderer
{
    private Dictionary<string, Control> _globalDefs = new();
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
                HandlDefs(defs);
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

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(scaleX, scaleY));
            fragmentContainer.RenderTransform = transformGroup;
        }
        foreach (var elem in frag.Children)
        {
            if (elem is SvgGroup g)
            {
                fragmentContainer.Children.Add(RenderGroup(g));
            }
            else throw new UnhandledElementException(elem);
        }

        _renderDom.Add(fragmentContainer);
    }

    private Control RenderGroup(SvgGroup g)
    {
        LogicalSvgGroup group = new();
        var transformGroup = new TransformGroup();
        group.Id = g.ID;
        group.Class = g.GetClasses();
        group.RenderTransform = g.ToAvaloniaTransformGroup();
        foreach (var elem in g.Children)
        {
            if (elem is SvgGroup _g)
            {
                group.Children.Add(RenderGroup(_g));
            }
            else if (elem is SvgPath _path)
            {
                group.Children.Add(_path.ToAvaloniaPath());
            }
            else if (elem is SvgText _text) { } //fuck this for now
            else if (elem is SvgUse _use)
            {
                //will always be a path group
                var dictEnry = _use.ReferencedElement.OriginalString.TrimStart('#');
                Control? path;
                if (_globalDefs.TryGetValue(dictEnry, out path) && path is Path p)
                {
                    LogicalSvgGroup useGroup = new();
                    useGroup.RenderTransform = _use.ToAvaloniaTransformGroup();
                    useGroup.Children.Add(p.ShallowClone());
                    group.Children.Add(useGroup);
                }
            }
            else if (elem is SvgPolygon _polygon)
            {
                group.Children.Add(_polygon.ToAvaloniaPolygon());
            }
            else if (elem is SvgEllipse _ellipse)
            {
                group.Children.Add(_ellipse.ToAvaloniaEllipse());
            }
            else {
                Console.WriteLine(elem.GetXML());
                Console.WriteLine();
                Console.WriteLine();
            }
            // else throw new UnhandledElementException(elem);
        }
        return group;
    }

    private void HandlDefs(SvgDefinitionList defs)
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
                        _globalDefs[groupId] = p.ToAvaloniaPath(); //TODO: Should be in a logical group
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
            foreach (var transform in self.Transforms)
            {
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
        path.Fill = Brushes.Black;
        path.Stroke = Brushes.Black;
        path.Stretch = Stretch.None;
        path.Data = geometry;
        path.StrokeLineCap = self.StrokeLineCap.ToAvaloniaPenLineCap();
        path.StrokeJoin = self.StrokeLineJoin.ToAvaloniaPenLineJoin();
        path.StrokeThickness = (double)self.StrokeWidth.Value;
        return path;
    }

    public static Avalonia.Controls.Shapes.Path ShallowClone(this Path self)
    {
        return new()
        {
            Data = self.Data,
            RenderTransform = self.RenderTransform,
            Stroke = self.Stroke,
        };
    }

    public static Avalonia.Controls.Shapes.Ellipse ToAvaloniaEllipse(this SvgEllipse self)
    {
        var ellipse = new Ellipse()
        {
            Width = self.RadiusX * 2,
            Height = self.RadiusY * 2,
            Fill = Brushes.Black,
            Stroke = Brushes.Black,
            Opacity = 1.0,
        };
        Canvas.SetLeft(ellipse, self.CenterX - self.RadiusX);
        Canvas.SetTop(ellipse, self.CenterY - self.RadiusY);
        return ellipse;
    }
    public static Avalonia.Controls.Shapes.Polygon ToAvaloniaPolygon(this SvgPolygon self)
    {
        var polygon = new Polygon()
        {
            Fill = Brushes.Black,
            Stroke = Brushes.Black,
            Opacity = 1.0,
        };
        var points = new Points();

        for (int i = 0; i < self.Points.Count - 1; i += 2)
        {
            double x = (double)self.Points[i].Value;
            double y = (double)self.Points[i + 1].Value;
            points.Add(new Point(x, y));
        }
        polygon.Points = points;
        return polygon;
    }
}
