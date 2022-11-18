using Microsoft.EntityFrameworkCore;
using PSXLink.MVVM.Commands;
using PSXLink.MVVM.Data;
using PSXLink.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PSXLink.MVVM.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        private readonly UpdateRepository _repository;
        private string? _range;

        private ICommand? _saveSetting;
        private ICommand? _start;
        private ICommand? _clearLogs;
        private ICommand? _logFolder;

        public LogViewModel()
        {
            _repository = new();
            SettingRepository.LoadSetting();
            FolderRepository.CreateFolder();
        }

        public ObservableCollection<UpdateLog> LogList { get; } = new();

        public bool CheckOnly
        {
            get => Setting.Instance().CheckOnly;
            set
            {
                Setting.Instance().CheckOnly = value;
                OnPropertyChanged();
            }
        }

        public bool CheckVersion
        {
            get => Setting.Instance().CheckVersion;
            set
            {
                Setting.Instance().CheckVersion = value;
                OnPropertyChanged();
            }
        }

        public string? Range
        {
            get => _range;
            set
            {
                _range = value;
                OnPropertyChanged();
            }
        }

        public ICommand? SaveSetting
        {
            get
            {
                _saveSetting ??= new RelayCommand(SaveSettingCommand);
                return _saveSetting;
            }
        }

        private void SaveSettingCommand(object? obj)
        {
            SettingRepository.SaveSetting(Setting.Instance());
        }

        public ICommand? Start
        {
            get
            {
                _start ??= new RelayCommand(StartCommand, CanStartCommand);
                return _start;
            }
        }

        private bool CanStartCommand(object? obj)
        {
            return Range?.Length > 0;
        }

        private async Task CheckByTitleID(string[] range)
        {
            using PSXLinkDataContext dbContext = new();
            for (int i = 1; i < range.Length; i++)
            {
                Game? game = await dbContext.Set<Game>().FirstOrDefaultAsync(g => g.TitleID == range[i]);
                if (game != null)
                {
                    UpdateLog log = await _repository.CheckUpdate(game, CheckVersion);
                    LogList.Add(log);
                    OnPropertyChanged(nameof(LogList));
                    if (!CheckOnly && log.Link?.Length > 1)
                    {
                        List<Game>? otherGames = await dbContext.Set<Game>().Where(o => o.ID != game.ID && o.Title.Equals(game.Title)).ToListAsync();
                        if (otherGames.Any())
                        {
                            foreach (Game other in otherGames)
                            {
                                log = await _repository.CheckUpdate(other, CheckVersion);
                                LogList.Add(log);
                                OnPropertyChanged(nameof(LogList));
                            }
                        }
                    }
                }
                else
                {
                    UpdateLog log = new()
                    {
                        Status = $"TitleID {range[i]} is Not Valid or Not Exist in Database"
                    };

                    LogList.Add(log);
                    OnPropertyChanged(nameof(LogList));
                }
            }
        }

        private async Task CheckByID(int start, int end, int region)
        {
            using PSXLinkDataContext dbContext = new();

            List<Game> games = await dbContext.Set<Game>().Where(g => g.ID >= start && g.ID <= end).Where(r => r.Region == region).ToListAsync();

            if (start > end)
            {
                UpdateLog log = new()
                {
                    Status = $"Range is Not Valid. {start} is Greater Than {end}"
                };

                LogList.Add(log);
                OnPropertyChanged(nameof(LogList));
                return;
            }

            if (games.Count == 0)
            {
                IsNotValid();
                return;
            }

            foreach (Game game in games)
            {
                UpdateLog log = await _repository.CheckUpdate(game, CheckVersion);
                LogList.Add(log);
                OnPropertyChanged(nameof(LogList));
                if (!CheckOnly && log.Link?.Length > 1)
                {
                    List<Game>? otherGames = await dbContext.Set<Game>().Where(o => o.ID != game.ID && o.Title == game.Title).ToListAsync();
                    if (otherGames != null && otherGames.Any())
                    {
                        foreach (Game other in otherGames)
                        {
                            log = await _repository.CheckUpdate(other, CheckVersion);
                            LogList.Add(log);
                            OnPropertyChanged(nameof(LogList));
                        }
                    }
                }
            }
        }

        private async Task CheckNewGame(int start, int end, int region)
        {
            using PSXLinkDataContext dbContext = new();

            List<Game> games = await dbContext.Set<Game>().Where(g => g.ID >= start && g.ID <= end).Where(r => r.Region == region).ToListAsync();

            if (start > end)
            {
                UpdateLog log = new()
                {
                    Status = $"Range is Not Valid. {start} is Greater Than {end}"
                };

                LogList.Add(log);
                OnPropertyChanged(nameof(LogList));
                return;
            }

            if (games.Count == 0)
            {
                IsNotValid();
                return;
            }

            foreach (Game game in games)
            {
                UpdateLog log = await _repository.NewGame(game);
                LogList.Add(log);
                OnPropertyChanged(nameof(LogList));
                await Task.Delay(200);
            }

        }

        private void IsNotValid()
        {
            UpdateLog log = new()
            {
                Status = $"Range Format is Not Valid or Not Exist in Database"
            };

            LogList.Add(log);
            OnPropertyChanged(nameof(LogList));
        }

        private async void StartCommand(object? obj)
        {
            Stopwatch watch = new();
            watch.Start();
            UpdateLog log = new()
            {
                Status = $"Operation Started"
            };

            LogList.Add(log);
            OnPropertyChanged(nameof(LogList));
            using PSXLinkDataContext dbContext = new();

            string[]? range = Range?.ToUpper().Trim().Split(' ');

            if (range != null && range.Length > 1)
            {
                if (range[0] == "C")
                {
                    await CheckByTitleID(range);
                }

                else if (range[0] == "T")
                {
                    range = range.Select(x => x = $"CUSA{x}").ToArray();
                    await CheckByTitleID(range);
                }

                else if (range[0] == "I" && range.Length == 4)
                {
                    bool num1 = int.TryParse(range[1], out int start);
                    bool num2 = int.TryParse(range[2], out int end);
                    if (num1 && num2)
                    {
                        int region = range[3] switch
                        {
                            "R1" => 1,
                            "R2" => 2,
                            "R3" => 3,
                            _ => 0
                        };
                        if (region > 0)
                        {
                            await CheckByID(start, end, region);
                        }
                        else
                        {
                            IsNotValid();
                        }
                    }
                    else
                    {
                        IsNotValid();
                    }
                }

                else if (range[0] == "N" && range.Length == 3)
                {
                    bool num1 = int.TryParse(range[1], out int start);
                    bool num2 = int.TryParse(range[2], out int end);
                    if (num1 && num2)
                    {
                        await CheckNewGame(start, end, 0);
                    }
                    else
                    {
                        IsNotValid();
                    }
                }

                else
                {
                    IsNotValid();
                }
            }
            else
            {
                IsNotValid();
            }
            watch.Stop();
            UpdateLog status = new()
            {
                Status = $"Operation Complete in {watch.Elapsed}"
            };
            _repository.AddLog(status);
            OnPropertyChanged(nameof(LogList));
        }

        public ICommand? ClearLogs
        {
            get
            {
                _clearLogs ??= new RelayCommand(ClearLogCommand, CanClearLogsCommand);
                return _clearLogs;
            }
        }

        private bool CanClearLogsCommand(object? obj)
        {
            return LogList.Count > 0;
        }

        private void ClearLogCommand(object? obj)
        {
            LogList.Clear();
            OnPropertyChanged(nameof(LogList));
        }

        public ICommand? LogFolder
        {
            get
            {
                _logFolder ??= new RelayCommand(LogFolderCommand);
                return _logFolder;
            }
        }

        private void LogFolderCommand(object? obj)
        {
            FolderRepository.OpenFolder(AppContext.BaseDirectory);
        }
    }
}
