using System;
using System.ComponentModel;
using System.IO;

namespace TTFViewer.Model
{
    public class TTFModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public event EventHandler<ErrorEventArgs> ErrorEvent;

        private void RaiseErrorEvent(Exception exception)
        {
            ErrorEvent?.Invoke(this, new ErrorEventArgs(exception));
        }


        TTFItemModel _RootItemModel;
        public TTFItemModel RootItemModel
        {
            get
            {
                return _RootItemModel;
            }
            private set
            {
                if(value != _RootItemModel)
                {
                    _RootItemModel = value;
                    RaisePropertyChanged("RootItemModel");
                }
            }
        }


        public string FilePath { get; private set; }


        public void Open(string path)
        {
            Close();

            FileStream fs = null;

            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            }
            catch(Exception e)
            {
                RaiseErrorEvent(e);
            }

            if (fs != null)
            {
                BinaryReader reader = new BinaryReader(fs);
                FilePath = path;
                RootItemModel = new RootItemModel(reader);
            }
        }


        public void Close()
        {
            FilePath = null;
            RootItemModel = null;
        }
    }
}
