using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Tabidus.POC.GUI.Common
{
    public class ReadOnlyPanel
    {
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.RegisterAttached(
                "IsReadOnly", typeof(bool), typeof(ReadOnlyPanel),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.Inherits, ReadOnlyPropertyChanged));

        public static bool GetIsReadOnly(DependencyObject o)
        {
            return (bool)o.GetValue(IsReadOnlyProperty);
        }

        public static void SetIsReadOnly(DependencyObject o, bool value)
        {
            o.SetValue(IsReadOnlyProperty, value);
        }

        private static void ReadOnlyPropertyChanged(
            DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is TextBox)
            {
                ((TextBox)o).IsReadOnly = (bool)e.NewValue;
            }
            if (o is ComboBox)
            {
                ((ComboBox)o).IsEnabled = !(bool)e.NewValue;
            }
            if (o is Button)
            {
                ((Button)o).IsEnabled = !(bool)e.NewValue;
            }
            if (o is CheckBox)
            {
                ((CheckBox)o).IsEnabled = !(bool)e.NewValue;
            }
            // other types here
        }
    }
}
