using PSXLink.MVVM.Commands;
using PSXLink.MVVM.Data;
using PSXLink.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PSXLink.MVVM.ViewModels
{
    public class PSXLinkViewModel:ViewModelBase
    {
        private WindowState _windowState;

        private ICommand? _close;
        private ICommand? _maximize;
        private ICommand? _minimize;

        public double MaxHeight => SystemParameters.WorkArea.Height;
        public double MaxWidth => SystemParameters.WorkArea.Width;

        public WindowState WindowState
        {
            get=> _windowState;
            set
            {
                _windowState = value;
                OnPropertyChanged();
            }
        }

        public ICommand? Close
        {
            get
            {
                _close ??= new RelayCommand(CloseCommand);
                return _close;
            }
        }

        private void CloseCommand(object? obj)
        {
            Application.Current.Shutdown();
        }

        public ICommand? Maximize
        {
            get
            {
                _maximize ??= new RelayCommand(MaximizeCommand);
                return _maximize;
            }
        }

        private void MaximizeCommand(object? obj)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        public ICommand? Minimize
        {
            get
            {
                _minimize ??= new RelayCommand(MinimizeCommand);
                return _minimize;
            }
        }

        private void MinimizeCommand(object? obj)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
