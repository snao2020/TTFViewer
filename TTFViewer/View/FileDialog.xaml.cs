using GuiMisc;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace TTFViewer.View
{
    public enum Ext
    {
        ttc,
        ttf,
        all
    }

    public partial class FileDialog : Window
    {
        public FileInfo FileInfo { get; private set; }

        static int SelectedIndex = 0;


        public FileDialog()
        {
            InitializeComponent();

            DataContext = this;
        }


        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var wp = Properties.Settings.Default.FileDialogPlacement;
            this.SetWindowPlacement(wp);

            Properties.Settings.Default.PropertyChanged += OnPropertyChanged;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FileInfo = null;
            Load();
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                                       new Action<object, int>(SetKeyboardFocusToItem), FileListBox, SelectedIndex);
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SelectedIndex = FileListBox.SelectedIndex;
            Properties.Settings.Default.PropertyChanged -= OnPropertyChanged;

            var wp = this.GetWindowPlacement();
            Properties.Settings.Default.FileDialogPlacement = wp;
        }


        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FileDialogExt")
                Load();
        }


        private void ButtonOk(object sender, RoutedEventArgs e)
        {
            if (FileListBox.SelectedItem is FileInfo fi)
                FileInfo = fi;

            DialogResult = true;
        }


        private void ButtonCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }


        void ListBoxItemDoubleClicked(object sender, RoutedEventArgs e)
        {
            if (FileListBox.SelectedItem is FileInfo fi)
                FileInfo = fi;

            DialogResult = true;
        }


        private void RadioStackPanel_GotFocus(object sender, RoutedEventArgs e)
        {
            if (e.Source is RadioButton rd)
                rd.IsChecked = true;
        }


        void Load()
        {
            string[] filter;
            switch (Properties.Settings.Default.FileDialogExt)
            {
                case Ext.ttc: filter = new string[] { ".ttc" }; break;
                case Ext.ttf: filter = new string[] { ".ttf" }; break;
                default: filter = new string[] { ".ttc", ".ttf" }; break;
            }

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

            DirectoryInfo dirInfo = new DirectoryInfo(path);

            var files = from f in dirInfo.EnumerateFiles()
                        where (filter.Contains<string>(f.Extension)) // == ".ttf" || f.Extension ==".ttc")
                        select f;

            FileListBox.ItemsSource = files;
            FileListBox.SelectedIndex = 0;
        }


        static void SetKeyboardFocusToItem(object parameter, int index)
        {
            if (parameter is ListBox lb)
            {
                lb.SelectedIndex = index;
                
                var gen = lb.ItemContainerGenerator;
                if (gen != null)
                {
                    if (gen.ContainerFromIndex(index) is IInputElement item)
                    {   
                        Keyboard.Focus(item);
                    }
                }
            }
        }
    }

    public class ExtConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return parameter;
            else
                return Binding.DoNothing;
        }
    }
}
