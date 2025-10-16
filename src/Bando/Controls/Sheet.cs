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
        // foreach (var child in Children)
        // {
        //     if (child is Image image)
        //     {
        //         if (image.Source is IDisposable disposable)
        //         {
        //             disposable.Dispose();
        //         }
        //         image.Source = null;
        //     }
        // }
        Children.Clear();
    }

    //Purge should be called before this
    public void SetPageCount(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Children.Add(new Canvas()
            {
            });
        }
    }
}
