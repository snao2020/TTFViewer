using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace GuiMisc
{
    [MarkupExtensionReturnType(typeof(double))]
    class CalcComboWidthExtension : MarkupExtension
    {
        ComboBox ComboBox;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget pvt)
            {
                if (pvt.TargetObject is ComboBox cb)
                {
                    if (DesignerProperties.GetIsInDesignMode(cb))
                        return 0.0;
                    ComboBox = cb;
                    cb.Loaded += Loaded;
                    cb.Unloaded += Unloaded;
                    return CalcWidth(cb);
                }
            }
            throw new ArgumentException();
        }


        private void Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var dpd0 = DependencyPropertyDescriptor.FromProperty(ComboBox.FontFamilyProperty, typeof(ComboBox));
            if (dpd0 != null)
                dpd0.AddValueChanged(ComboBox, PropertyChanged);
            var dpd1 = DependencyPropertyDescriptor.FromProperty(ComboBox.FontStyleProperty, typeof(ComboBox));
            if (dpd1 != null)
                dpd1.AddValueChanged(ComboBox, PropertyChanged);
            var dpd2 = DependencyPropertyDescriptor.FromProperty(ComboBox.FontWeightProperty, typeof(ComboBox));
            if (dpd2 != null)
                dpd2.AddValueChanged(ComboBox, PropertyChanged);
            var dpd3 = DependencyPropertyDescriptor.FromProperty(ComboBox.FontSizeProperty, typeof(ComboBox));
            if (dpd3 != null)
                dpd3.AddValueChanged(ComboBox, PropertyChanged);
            if (ComboBox.ItemsSource is INotifyCollectionChanged ncc)
                ncc.CollectionChanged += CollectionChanged;
        }

        private void Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var dpd0 = DependencyPropertyDescriptor.FromProperty(ComboBox.FontFamilyProperty, typeof(ComboBox));
            if (dpd0 != null)
                dpd0.RemoveValueChanged(ComboBox, PropertyChanged);
            var dpd1 = DependencyPropertyDescriptor.FromProperty(ComboBox.FontStyleProperty, typeof(ComboBox));
            if (dpd1 != null)
                dpd1.RemoveValueChanged(ComboBox, PropertyChanged);
            var dpd2 = DependencyPropertyDescriptor.FromProperty(ComboBox.FontWeightProperty, typeof(ComboBox));
            if (dpd2 != null)
                dpd2.RemoveValueChanged(ComboBox, PropertyChanged);
            var dpd3 = DependencyPropertyDescriptor.FromProperty(ComboBox.FontSizeProperty, typeof(ComboBox));
            if (dpd3 != null)
                dpd3.RemoveValueChanged(ComboBox, PropertyChanged);
            if (ComboBox.ItemsSource is INotifyCollectionChanged ncc)
                ncc.CollectionChanged -= CollectionChanged;
        }


        private static void PropertyChanged(object sender, EventArgs e)
        {
            if(sender is ComboBox cb)
                cb.Width = CalcWidth(cb);
        }


        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ComboBox.Width = CalcWidth(ComboBox);
        }


        private static double CalcWidth(ComboBox cb)
        {
            var tb = new TextBlock
            {
                FontFamily = cb.FontFamily,
                FontStyle = cb.FontStyle,
                FontWeight = cb.FontWeight,
                FontSize = cb.FontSize,
            };

            double w = 0.0;
            foreach (var item in cb.ItemsSource)
            {
                tb.Text = item.ToString();
                tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                if (tb.DesiredSize.Width > w)
                {
                    w = tb.DesiredSize.Width;
                }
            }
            return w + 25.0; // combobox.padding=(4,3,4,3) arrow's parent grid width=17
        }
    }
}
