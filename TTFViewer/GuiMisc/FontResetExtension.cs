using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace GuiMisc
{
    class ResourceKeySettingCollection : ObservableCollection<ResourceKeySetting>
    {
    }


    [ContentProperty("Collection")]
    [MarkupExtensionReturnType(typeof(bool))]
    class FontResetExtension : MarkupExtension, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public ResourceKeySettingCollection Collection { get; } = new ResourceKeySettingCollection();

        bool _IsChecked;
        public bool IsChecked
        {
            get => _IsChecked;
            set
            {
                if (value != _IsChecked)
                {
                    foreach (var i in Collection)
                        i.IsDefault = value;
                    _IsChecked = value;
                    RaisePropertyChanged("IsChecked");
                }
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget pvt)
            {
                if (pvt.TargetObject is FrameworkElement fe && pvt.TargetProperty is DependencyProperty dp)
                {
                    if (DesignerProperties.GetIsInDesignMode(fe))
                        return false;
                    bool isChecked = true;
                    foreach (var i in Collection)
                    {
                        if (!i.IsDefault)
                            isChecked = false;
                        i.PropertyChanged += ResourceKeySettingChanged;
                    }
                    _IsChecked = isChecked;
                    
                    var binding = new Binding("IsChecked")
                    {
                        Source = this,
                        Mode=BindingMode.TwoWay,
                    };
                    fe.SetBinding(dp, binding);
                    return binding.ProvideValue(serviceProvider);                    
                }
            }
            return IsChecked;
        }


        private void ResourceKeySettingChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ResourceKeySetting rks)
            {
                if (e.PropertyName == "IsDefault")
                {
                    bool isChecked = rks.IsDefault;
                    IsChecked = isChecked;
                }
            }
        }
    }
}
