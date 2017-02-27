using Esri.ArcGISRuntime.Mapping;
using System;
using System.Windows.Input;

namespace OfflineMapBook.Commands
{
    public class ParameterCommand : ICommand
    {
        private Action<object> _action;
        private bool _canExecute;
        private object _object;

        public ParameterCommand(Action<object> action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;

        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _object = parameter as object;
            _action(_object);
        }
    }
}