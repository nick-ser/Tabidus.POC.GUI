using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Tabidus.POC.GUI.Common
{
    public class Tracker : ContentControl
    {
        public static readonly DependencyProperty IsChangedProperty =
            DependencyProperty.Register("IsChanged", typeof (bool), typeof (Tracker), new PropertyMetadata(false));

        public static readonly DependencyProperty PropertyProperty =
            DependencyProperty.RegisterAttached("Property", typeof (string), typeof (Tracker),
                new PropertyMetadata(null));

        private readonly List<PropertyWatcher> changedProperties = new List<PropertyWatcher>();
        private readonly object nullProperty = new object();

        private readonly Dictionary<PropertyWatcher, object> trackedPropertySnapshot =
            new Dictionary<PropertyWatcher, object>();

        public Tracker()
        {
            Loaded += (s, e) =>
            {
                WalkDownVisualTree();
            };
        }

        public bool IsChanged
        {
            get { return (bool) GetValue(IsChangedProperty); }
            set { SetValue(IsChangedProperty, value); }
        }

        private void WalkDownVisualTree()
        {
            var content = Content as DependencyObject;
            if (content != null)
                WalkDownVisualTree(content);
        }

        public static string GetProperty(DependencyObject obj)
        {
            return (string) obj.GetValue(PropertyProperty);
        }

        public static void SetProperty(DependencyObject obj, string value)
        {
            obj.SetValue(PropertyProperty, value);
        }

        public void AcceptChanges()
        {
            changedProperties.Clear();

            IsChanged = false;
        }

        private void WalkDownVisualTree(DependencyObject current)
        {
            var property = current.ReadLocalValue(PropertyProperty);
            if (property != DependencyProperty.UnsetValue)
                RegisterTrackedProperty(current, (string) property);

            var count = VisualTreeHelper.GetChildrenCount(current);
            for (var i = 0; i < count; i++)
            {
                WalkDownVisualTree(VisualTreeHelper.GetChild(current, i));
            }
        }

        private void RegisterTrackedProperty(DependencyObject item, string propertyName)
        {
            var notifier = new PropertyWatcher(item, propertyName);
            notifier.ValueChanged += TrackedPropertyChanged;

            trackedPropertySnapshot.Add(notifier, notifier.Value);
        }

        private void TrackedPropertyChanged(object sender, EventArgs e)
        {
            var notifier = (PropertyWatcher) sender;
            var original = trackedPropertySnapshot[notifier] ?? nullProperty;
            var current = notifier.Value ?? nullProperty;

            if (!original.Equals(current))
            {
                if (!changedProperties.Contains(notifier))
                    changedProperties.Add(notifier);
            }
            else
                changedProperties.Remove(notifier);

            IsChanged = changedProperties.Count != 0;
        }
    }
}