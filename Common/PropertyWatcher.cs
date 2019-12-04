using System;
using System.Windows;
using System.Windows.Data;

namespace Tabidus.POC.GUI.Common
{
    public class PropertyWatcher : DependencyObject
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof (object), typeof (PropertyWatcher),
                new PropertyMetadata(null, OnPropertyChanged));

        public PropertyWatcher(DependencyObject source, string propertyName)
        {
            Source = source;

            var path = new PropertyPath(propertyName);
            var binding = new Binding {Path = path, Mode = BindingMode.OneWay, Source = source};
            BindingOperations.SetBinding(this, ValueProperty, binding);
        }

        public DependencyObject Source { get; protected set; }

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var notifier = (PropertyWatcher) sender;
            notifier.ValueChanged(notifier, EventArgs.Empty);
        }

        public event EventHandler ValueChanged = delegate { };
    }
}