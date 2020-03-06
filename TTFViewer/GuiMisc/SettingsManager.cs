using System;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace GuiMisc
{
    class SettingsManager
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void SettingChanged(object sender, PropertyChangedEventArgs e)
            => PropertyChanged?.Invoke(this, e);


        string SettingName;

        IValueConverter ValueConverter;

        public SettingsManager(string settingName, IValueConverter valueConverter)
        {
            SettingName = settingName;
            ValueConverter = valueConverter;

            GetSettings().SettingChanging += SettingChanging;
            GetSettings().PropertyChanged += SettingChanged;
        }


        private void SettingChanging(object sender, SettingChangingEventArgs e)
        {
            if (e.SettingName == SettingName)
            {
                object value = GetSettings()[e.SettingName];
                if (value == null ? e.NewValue == null : value.Equals(e.NewValue))
                {
                    e.Cancel = true;
                }
            }
        }


        public bool IsSettingDefault()
        {
            // Settings.PropertyValues[name] may be null
            // Calling Settings[name] validates Settings.PropertyValues[name]
            //
            object value = GetSettings()[SettingName];
            SettingsPropertyValue pv = GetSettings().PropertyValues[SettingName];

            return pv != null && IsDefault(pv);
        }


        public void SetSettingDefault()
        {
            // Settings.PropertyValues[name] may be null
            // Calling Settings[name] validates Settings.PropertyValues[name]
            //
            object obj = GetSettings()[SettingName];
            SettingsPropertyValue pv = GetSettings().PropertyValues[SettingName];

            if (pv != null && !IsDefault(pv))
            {
                GetSettings()[SettingName] = pv.Property.DefaultValue;
            }
        }


        public object GetSettingValue(Type valueType)
        {
            object value = GetSettings()[SettingName];

            if (value == null || value.GetType() == valueType)
                return value;

            else if (ValueConverter != null)
                return ValueConverter.ConvertBack(value, valueType, null, new System.Globalization.CultureInfo("en-US"));

            else
            {
                var converter = TypeDescriptor.GetConverter(valueType);
                if (converter != null && converter.CanConvertFrom(value.GetType()))
                    return converter.ConvertFrom(value);
                else
                    return null;
            }
        }


        public void SetSettingValue(object value)
        {
            if (value.GetType() != typeof(string) && GetSettings().Properties[SettingName].PropertyType == typeof(string))
            {

                if (ValueConverter != null)
                {
                    string old = (string)GetSettings()[SettingName];
                    value = ValueConverter.Convert(value, typeof(string), null, new System.Globalization.CultureInfo("en-US"));
                    if ((string)value == old)
                        return;
                }
                else
                {
                    string old = (string)GetSettings()[SettingName];
                    var converter = TypeDescriptor.GetConverter(value.GetType());
                    if (converter != null)
                    {
                        value = converter.ConvertTo(value, typeof(string));
                        if ((string)value == old)
                            return;
                    }
                }
            }
            else if (value == GetSettings()[SettingName])
                return;

            GetSettings()[SettingName] = value;
        }


        static bool IsDefault(SettingsPropertyValue pv)
        {
            bool result = false;

            object defValue = pv.Property.DefaultValue;
            object serialized = pv.SerializedValue;

            if (defValue == null)
            {
                result = serialized == null || serialized is string s && s == "";
            }
            else if (defValue is string d)
            {
                result = serialized is string s && s == d;
            }

            return result;
        }


        static private ApplicationSettingsBase _Settings;
        static ApplicationSettingsBase GetSettings()
        {
            if (_Settings == null)
            {
                Type appType = Application.Current.GetType();
                string name = appType.Namespace + ".Properties.Settings";
                Type type = appType.Assembly.GetType(name);
                if (type != null && type.IsSubclassOf(typeof(ApplicationSettingsBase)))
                {
                    PropertyInfo info = type.GetProperty("Default", BindingFlags.Static | BindingFlags.Public);
                    _Settings = info.GetValue(null) as ApplicationSettingsBase;
                }
                else
                    throw new SettingsNotFoundException();
            }
            return _Settings;
        }
    }
}
