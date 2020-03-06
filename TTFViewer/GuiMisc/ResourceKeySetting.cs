using System;
using System.ComponentModel;
using System.Windows.Data;

namespace GuiMisc
{
    class SettingsNotFoundException : Exception
    {
    }
    

    public class ResourceKeySetting : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        Type _ValueType;
        public Type ValueType
        {
            get => _ValueType;
            set => _ValueType = _ValueType ?? value;
        }


        object _ResourceKey;
        public object ResourceKey
        {
            get => _ResourceKey;
            set => _ResourceKey = _ResourceKey ?? value;
        }


        string _SettingName;
        public string SettingName
        {
            get => _SettingName;
            set
            {
                if (_SettingName == null && !string.IsNullOrEmpty(value))
                    _SettingName = value;
            }
        }


        IValueConverter _ValueConverter;
        public IValueConverter ValueConverter
        {
            get => _ValueConverter;
            set => _ValueConverter = _ValueConverter ?? value;
        }


        object _Value;
        public object Value
        {
            get
            {
                Initialize();
                return _Value;
            }
            set
            {
                if (!value.Equals(_Value))
                {
                    _Value = value;
                    IsDefault = false;
                    if (SettingsManager != null)
                        SettingsManager.SetSettingValue(value);
                    RaisePropertyChanged("Value");
                }
                else
                    IsDefault = false;
            }
        }


        bool _IsDefault = true;
        public bool IsDefault
        {
            get
            {
                Initialize();
                return _IsDefault;
            }
            set
            {
                if (value != _IsDefault)
                {
                    _IsDefault = value;
                    if (SettingsManager != null)
                    {
                        if (value)
                        {
                            _IsDefault = false;
                            Value = ResourceValueProvider.Value;
                            _IsDefault = true;
                            SettingsManager.SetSettingDefault();
                        }
                        else
                            SettingsManager.SetSettingValue(Value);
                    }
                    RaisePropertyChanged("IsDefault");
                }
            }
        }


        SettingsManager SettingsManager;
        ResourceValueProvider ResourceValueProvider;

        bool Initialized = false;
        private void Initialize()
        {
            if (!Initialized)
            {
                Initialized = true;

                if (SettingsManager == null && !string.IsNullOrEmpty(SettingName))
                {
                    SettingsManager = new SettingsManager(SettingName, ValueConverter);
                }

                if (SettingsManager != null)
                    SettingsManager.PropertyChanged += SettingsChanged;

                if (ResourceValueProvider == null && ResourceKey != null)
                {
                    ResourceValueProvider = new ResourceValueProvider { ResourceKey = ResourceKey };
                    if (ValueType == null)
                        ValueType = ResourceValueProvider.Value.GetType();
                    ResourceValueProvider.PropertyChanged += ResourceValueChanged;
                }
                _IsDefault = SettingsManager.IsSettingDefault();
                if (_IsDefault)
                    _Value = ResourceValueProvider.Value;
                else
                    _Value = SettingsManager.GetSettingValue(ValueType);
            }
        }


        private void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (SettingsManager.IsSettingDefault())
            {
                IsDefault = true;
            }
            else
            {
                Value = SettingsManager.GetSettingValue(ValueType);
            }
        }


        private void ResourceValueChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsDefault)
            {
                Value = ResourceValueProvider.Value;
                IsDefault = true;
            }
        }
    }
}
