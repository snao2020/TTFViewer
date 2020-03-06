using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using TTFViewer.Model;

namespace TTFViewer.ViewModel
{
    public class TTFViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public event EventHandler<ErrorEventArgs> ErrorEvent;

        private void RaiseErrorEvent(ErrorEventArgs e)
        {
            ErrorEvent?.Invoke(this, e);
        }


        ItemViewModel _ItemViewModel;
        public ItemViewModel ItemViewModel
        {
            get
            {
                return _ItemViewModel;
            }
            private set
            {
                if (value != _ItemViewModel)
                {
                    _ItemViewModel = value;
                    RaisePropertyChanged("ItemViewModel");
                }
            }
        }


        static readonly string StaticWindowTitle = "TTFViewer";
        string _WindowTitle = StaticWindowTitle;
        public string WindowTitle
        {
            get
            {
                return _WindowTitle;
            }
            private set
            {
                if (value != _WindowTitle)
                {
                    _WindowTitle = value;
                    RaisePropertyChanged("WindowTitle");
                }
            }
        }


        public RelayCommand OpenCommand { get; }
        public RelayCommand CloseCommand { get; }


        TTFModel TTFModel;

        public TTFViewModel(TTFModel model)
        {
            TTFModel = model;
            OpenCommand = new RelayCommand(OnOpenCommand, null);
            CloseCommand = new RelayCommand(OnCloseCommand, (o) => { return TTFModel.RootItemModel != null; });

            TTFModel.PropertyChanged += OnModelPropertyChanged;
            TTFModel.ErrorEvent += OnError;
        }


        void OnOpenCommand(object parameter)
        {
            if (parameter is string path)
                TTFModel.Open(path);
        }


        void OnCloseCommand(object parameter)
        {
            TTFModel.Close();
        }


        void OnError(object sender, ErrorEventArgs e)
        {
            RaiseErrorEvent(e);
        }


        void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "RootItemModel":
                    RootItemModelPropertyChanged();
                    break;
            }
        }


        void RootItemModelPropertyChanged()
        {
            CloseCommand.RaiseCanExecuteChanged();

            if (TTFModel.RootItemModel != null)
            {
                ItemViewModel = new TTFItemViewModel(TTFModel.RootItemModel)
                {
                    IsExpanded = true,
                };
                string filePath = TTFModel.FilePath;
                if (filePath != null)
                    WindowTitle = string.Format("{0} {1}", Path.GetFileName(filePath), StaticWindowTitle);
            }
            else
            {
                WindowTitle = StaticWindowTitle;
                ItemViewModel = null;
            }
        }
    }
}
