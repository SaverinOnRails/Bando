using System;
using Avalonia.Controls;

namespace Bando.Controls;

public class Sheet : StackPanel
{
    public Sheet()
    {
        Orientation = Avalonia.Layout.Orientation.Vertical;
    }
    public void Purge()
    {
        Children.Clear();
    }

    public void SetPageCount(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Children.Add(new PageCanvas()
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Index = i,
            });
        }
    }
}

public class PageCanvas : Canvas
{
    public double? OriginalSvgHeight { get; set; } = null;
    public double? OriginalSvgWidth { get; set; } = null;
    public int Index { get; set; }
    public PageCanvas()
    {
        Margin = new(10);
    }
}
