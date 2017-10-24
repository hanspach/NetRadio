using System;
using System.Windows.Input;

namespace NetRadio.ViewModels
{
    class ActionCommand : ICommand
    {
        private readonly Action<object> executeHandler;
        private readonly Func<object, bool> canExecuteHandler;

        public ActionCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException("Execute can not be null");
            executeHandler = execute;
            canExecuteHandler = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            if (canExecuteHandler == null)
                return true;
            return canExecuteHandler(parameter);
        }

        public void Execute(object parameter)
        {
            executeHandler(parameter);
        }
    }
}
