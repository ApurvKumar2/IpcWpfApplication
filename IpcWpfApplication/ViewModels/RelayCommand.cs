using System;
using System.Windows.Input;

namespace IpcWpfApplication.ViewModels
{
    public class RelayCommand<T> : ICommand
    {
        readonly Predicate<T> _canexecute;
        readonly Action<T> _execute;

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        { }

        public RelayCommand(Action<T> execute, Predicate<T> canexecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canexecute = canexecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canexecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_canexecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        public Boolean CanExecute(Object parameter)
        {
            return _canexecute == null ? true : _canexecute((T)parameter);
        }

        public void Execute(Object parameter)
        {
            _execute((T)parameter);
        }
    }

    public class RelayCommand : ICommand
    {
        readonly Func<Boolean> _canexecute;
        readonly Action _execute;

        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action execute, Func<Boolean> canexecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canexecute = canexecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canexecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_canexecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        public Boolean CanExecute(Object parameter)
        {
            return _canexecute == null ? true : _canexecute();
        }

        public void Execute(Object parameter)
        {
            _execute();
        }
    }
}
