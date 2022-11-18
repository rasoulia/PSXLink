﻿using System;
using System.Windows.Input;

namespace PSXLink.MVVM.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?>? _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?>? execute) : this(execute, null) { }

        public RelayCommand(Action<object?>? execute, Predicate<object?>? canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute can not be null");
            }
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute?.Invoke(parameter);
        }
    }
}