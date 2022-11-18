using PSXLink.MVVM.Commands;
using PSXLink.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using PSXLink.MVVM.Data;
using WF = System.Windows.Forms;

namespace PSXLink.MVVM.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private Game? _game;

        private ICommand? _addGame;
        private ICommand? _editGame;
        private ICommand? _deleteGame;
        private ICommand? _backup;
        private ICommand? _restore;

        private readonly GameRepository _repository;

        public GameViewModel()
        {
            _game = new();

            _repository = new();
        }

        public ObservableCollection<Game> GameList => new(_repository.ReadAll().Result);

        public Game? Game
        {
            get => _game;
            set
            {
                _game = value;
                OnPropertyChanged();
            }
        }

        public ICommand? AddGame
        {
            get
            {
                _addGame ??= new RelayCommand(AddGameCommand, CanAddGameCommand);
                return _addGame;
            }
        }

        private bool CanAddGameCommand(object? obj)
        {
            return Game?.ID < 1 && Game.TitleID?.Length > 0 && Game.Title?.Length > 0;
        }

        private async void AddGameCommand(object? obj)
        {
            await _repository.Create(Game!);
            Game = new();
            OnPropertyChanged(nameof(GameList));
        }

        public ICommand? EditGame
        {
            get
            {
                _editGame ??= new RelayCommand(EditGameCommand, CanEditGameCommand);
                return _editGame;
            }
        }

        private bool CanEditGameCommand(object? obj)
        {
            return Game?.ID > 0 && Game.TitleID?.Length > 0 && Game.Title?.Length > 0;
        }

        private async void EditGameCommand(object? obj)
        {
            MessageBoxResult result = MessageBox.Show("Are You Sure?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
            if (result == MessageBoxResult.Yes)
            {
                await _repository.Update(Game!);
                OnPropertyChanged(nameof(GameList));
            }
            Game = new();
        }

        public ICommand? DeleteGame
        {
            get
            {
                _deleteGame ??= new RelayCommand(DeleteGameCommand, CanDeleteGameCommand);
                return _deleteGame;
            }
        }

        private bool CanDeleteGameCommand(object? obj)
        {
            return Game?.ID > 0;
        }

        private async void DeleteGameCommand(object? obj)
        {
            MessageBoxResult result = MessageBox.Show("Are You Sure?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
            if (result == MessageBoxResult.Yes)
            {
                await _repository.Delete(Game!);
                OnPropertyChanged(nameof(GameList));
            }
            Game = new();
        }

        public ICommand? Backup
        {
            get
            {
                _backup ??= new RelayCommand(BackupCommand);
                return _backup;
            }
        }

        private async void BackupCommand(object? obj)
        {
            await _repository.Backup();
        }

        public ICommand? Restore
        {
            get
            {
                _restore ??= new RelayCommand(RestoreCommand);
                return _restore;
            }
        }

        private void RestoreCommand(object? obj)
        {
            WF.OpenFileDialog ofd = new() 
            {
                Filter="Backup Json (*.json)|*.json"
            };
            if (ofd.ShowDialog() == WF.DialogResult.OK)
            {
                Task.Run(async () => await _repository.Restore(ofd.FileName));
            }
        }
    }
}
