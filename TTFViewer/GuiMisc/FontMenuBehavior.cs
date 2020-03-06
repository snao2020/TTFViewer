using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;


namespace GuiMisc
{
    public class FontMenuBehavior
    {
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItem", typeof(object), typeof(FontMenuBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                                              SelectedItemChanged));

        private static void SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MenuItem mi)
            {
                if (GetOriginalItemsSource(mi) == null)
                {
                    SetOriginalItemsSource(mi, mi.ItemsSource);
                    mi.ItemsSource = null;
                    mi.Items.Add(new MenuItem());
                    mi.SubmenuOpened += SubmenuOpened;
                    mi.Loaded += Loaded;
                    mi.Unloaded += Unloaded;
                    mi.Click += Click;
                }
                else if(e.OldValue != null && e.NewValue == null)
                {
                    SetOriginalItemsSource(mi, null);
                    mi.ItemsSource = null;
                    mi.Items.Clear();
                    mi.SubmenuOpened -= SubmenuOpened;
                    mi.Loaded -= Loaded;
                    mi.Unloaded -= Unloaded;
                    mi.Click -= Click;
                }
            }
        }

        public static object GetSelectedItem(MenuItem mi)
            => mi.GetValue(SelectedItemProperty);

        public static void SetSelectedItem(MenuItem mi, object value)
            => mi.SetValue(SelectedItemProperty, value);

        
        public static readonly DependencyProperty OriginalItemsSourceProperty =
            DependencyProperty.RegisterAttached(
                "OriginalItemsSource", typeof(IEnumerable), typeof(FontMenuBehavior));

        public static IEnumerable GetOriginalItemsSource(MenuItem mi)
            => (IEnumerable)mi.GetValue(OriginalItemsSourceProperty);
        
        public static void SetOriginalItemsSource(MenuItem mi, IEnumerable value)
            => mi.SetValue(OriginalItemsSourceProperty, value);



        static void Loaded(object sender, RoutedEventArgs e)
        {
            // Loaded event may be fired twice.
            if (e.Source is MenuItem mi && mi.ItemsSource == null)
            {
                mi.Items.Clear();
                mi.ItemsSource = GetOriginalItemsSource(mi);
            }
        }

        static void Unloaded(object sender, RoutedEventArgs e)
        {
            if (e.Source is MenuItem mi)
            {
                mi.ItemsSource = null;
                mi.Items.Add(new MenuItem());
            }
        }

        static void SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (e.Source is MenuItem mi)
            {
                mi.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        new Action<MenuItem>(BringEntireItemIntoView), mi);
            }
        }


        static void Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is MenuItem mi && e.OriginalSource is MenuItem child)
                SetSelectedItem(mi, child.DataContext);
        }


        static void BringEntireItemIntoView(MenuItem mi)
        {
            var icg = mi.ItemContainerGenerator;
            if (icg != null)
            {
                object selected = GetSelectedItem(mi);
                if(icg.ContainerFromItem(selected) is MenuItem selectedItem)
                {
                    if (selectedItem.Icon is RadioButton rb)
                        rb.IsChecked = true;
                    Rect targetRect = new Rect(0, 0, selectedItem.ActualWidth, selectedItem.ActualHeight + 20);
                    selectedItem.BringIntoView(targetRect);
                }
            }
        }
    }
}
