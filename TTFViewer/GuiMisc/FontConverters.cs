using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace GuiMisc
{
    public class FontFamilyComparer : IEqualityComparer
    {
        public new bool Equals(object x, object y)
        {
            if (x is FontFamily lhs && y is FontFamily rhs)
            {
                XmlLanguage us = XmlLanguage.GetLanguage("en-US");
                if(lhs.FamilyNames.TryGetValue(us, out string lhsName)
                    && rhs.FamilyNames.TryGetValue(us, out string rhsName))    
                {
                    return lhsName.Equals(rhsName);
                }
            }
            return false;
        }   


        public int GetHashCode(object obj)
        {
            if (obj is FontFamily ff)
            {
                XmlLanguage us = XmlLanguage.GetLanguage("en-US");
                if (ff.FamilyNames.TryGetValue(us, out string name))
                    return name.GetHashCode();
            }
            return obj.GetHashCode();
        }
    }


    public class EnumerableCollection : ObservableCollection<object>
    {
        public IEnumerable Source
        {
            set
            {
                if (Count == 0)
                {
                    foreach (var i in value)
                        Add(i);
                }
            }
        }

        public IEqualityComparer EqualityComparer { get; set; }
    }


    public class FontWeightComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            if(x is FontWeight lhs && y is FontWeight rhs)
                return FontWeight.Compare(lhs, rhs);
            return 0;
        }
    }

    public class ComparerCollection : ObservableCollection<object>
    {
        public IComparer Comparer { get; set; }
    }

    public class RangeCollection : ObservableCollection<double>
    {
        double _Min;
        public double Min
        {
            set
            {
                _Min = value;
                Load();
            }
        }

        double _Max;
        public double Max
        {
            set
            {
                _Max = value;
                Load();
            }
        }

        double _Step = 1.0;
        public double Step
        {
            set
            {
                _Step = value;
                Load();
            }
        }

        void Load()
        {
            Clear();
            for (double x = _Min; x <= _Max; x += _Step)
                Add(x);
        }
    }


    public class FontFamilyNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result = null;
            if (value is FontFamily ff)
            {
                XmlLanguage key = XmlLanguage.GetLanguage(culture.Name);
                if (key != null && ff.FamilyNames.TryGetValue(key, out string ret))
                    result = ret;
                else
                    result = ff.Source;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string name && targetType == typeof(FontFamily))
                return new FontFamily(name);
            else
                throw new NotImplementedException();
        }
    }


    class UpdateFontFamiliesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result = value;
            if (parameter is EnumerableCollection collection && collection.EqualityComparer != null)
            {
                bool found = false;
                foreach (object obj in collection)
                {
                    if (collection.EqualityComparer.Equals(obj, value))
                    {
                        found = true;
                        result = obj;
                        break;
                    }
                }
                if (!found)
                    collection.Add(value);
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class UpdateCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is ComparerCollection cc)
            {
                bool done = false;
                IComparer comparer = cc.Comparer;
                if (comparer != null)
                {
                    for (int i = 0; !done && i < cc.Count; i++)
                    {
                        int ret = comparer.Compare(cc[i], value);
                        if (ret == 0)
                            done = true;
                        else if(ret > 0)
                        {
                            cc.Insert(i, value);
                            done = true;
                        }
                    }
                }
                else
                    done = cc.Contains(value);

                if (!done)
                    cc.Add(value);

            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class UpdateFontSizesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                TypeConverter converter = new LengthConverter();
                double factor = (double)converter.ConvertFrom(1.ToString() + "pt");
                double result = Math.Round(d / factor, 3, MidpointRounding.AwayFromZero);
                
                if(parameter is ObservableCollection<double> collection)
                {
                    bool done = false;
                    int count = collection.Count;
                    for (int i = 0; !done && i < count; i++)
                    {
                        int ret = result.CompareTo(collection[i]);
                        if (ret == 0)
                            done = true;
                        else if (ret < 0)
                        {
                            collection.Insert(i, result);
                            done = true;
                        }
                    }
                    if(!done)
                        collection.Add(result);
                }
                
                return result;
            }
            else
                throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                TypeConverter converter = new LengthConverter();                
                double factor = (double)converter.ConvertFrom(1.ToString() + "pt");
                double result = Math.Round(d * factor, 3, MidpointRounding.AwayFromZero);
                return result;
            }
            else
                throw new NotImplementedException();
        }
    }
}
