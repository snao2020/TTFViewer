using System;
using System.Windows.Input;

namespace TTFViewer.ViewModel
{
    public class RelayCommand : ICommand
    {
        Action<object> ExecuteHandler;
        Func<object, bool> CanExecuteHandler;


        public RelayCommand(Action<object> executeHandler, Func<object, bool> canExecuteHandler)
        {
            ExecuteHandler = executeHandler;
            CanExecuteHandler = canExecuteHandler;
        }


        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }


        public event EventHandler CanExecuteChanged;


        public bool CanExecute(object parameter)
        {
            return CanExecuteHandler == null ? true : CanExecuteHandler(parameter);
        }


        public void Execute(object parameter)
        {
            ExecuteHandler?.Invoke(parameter);
        }
    }
}
