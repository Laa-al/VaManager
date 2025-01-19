using System.Windows;
using System.Windows.Controls;

namespace VaManager.Extensions;


public static class MarginSetter
{
    public static Thickness GetChildMargin(DependencyObject obj)
    {
        return (Thickness)obj.GetValue(ChildMarginProperty);
    }

    public static void SetChildMargin(DependencyObject obj, Thickness value)
    {
        obj.SetValue(ChildMarginProperty, value);
    }

    // Using a DependencyProperty as the backing store for Margin.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ChildMarginProperty =
        DependencyProperty.RegisterAttached("ChildMargin", typeof(Thickness), typeof(MarginSetter),
            new UIPropertyMetadata(new Thickness(), MarginChangedCallback));

    public static void MarginChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
    {
        // Make sure this is put on a panel
        if (sender is not Panel panel) return;

        panel.Loaded += OnPanelLoaded;
    }

    private static void OnPanelLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Panel panel) return;

        // Go over the children and set margin for them:
        foreach (var child in panel.Children)
        {
            if (child is not FrameworkElement fe) continue;

            fe.Margin = GetChildMargin(panel);
        }
    }
}