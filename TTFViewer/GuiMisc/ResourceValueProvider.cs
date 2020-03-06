using System.ComponentModel;
using System.Windows;

namespace GuiMisc
{
    class ResourceValueProvider : Freezable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value", typeof(object), typeof(ResourceValueProvider),
                new FrameworkPropertyMetadata(ValueChanged));

        public object Value
        {
            get => GetValue(ValueProperty);
            private set => SetValue(ValueProperty, value);
        }

        static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is ResourceValueProvider rvp)
            {
                rvp.RaisePropertyChanged("Value");
            }
        }


        public static readonly DependencyProperty ResourceKeyProperty =
            DependencyProperty.Register(
                "ResourceKey", typeof(object), typeof(ResourceValueProvider),
                new FrameworkPropertyMetadata(
                    null, CoerceResourceKeyCallback));


        private static object CoerceResourceKeyCallback(DependencyObject d, object baseValue)
        {
            if (d is ResourceValueProvider rvp)
            {
                object curr = rvp.ResourceKey;
                if(curr == null && baseValue != null)
                {
                    var dre = new DynamicResourceExtension(baseValue);
                    rvp.Value = dre.ProvideValue(null);
                    return baseValue;
                }
                return curr;
            }
            return baseValue;
        }


        public object ResourceKey
        {
            private get => GetValue(ResourceKeyProperty);
            set => SetValue(ResourceKeyProperty, value);
        }


        protected override Freezable CreateInstanceCore()
            => new ResourceValueProvider();
    }
}
