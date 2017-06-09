using Captura.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace Captura.ViewModels
{
    public class RecentItemViewModel : ViewModelBase
    {
        public RecentItemViewModel(string FilePath, RecentItemType ItemType, bool IsSaving)
        {
            this.FilePath = FilePath;

            FileName = Path.GetFileName(FilePath);

            this.IsSaving = IsSaving;
            this.ItemType = ItemType;

            if (ItemType == RecentItemType.Video && !Path.HasExtension(FilePath))
            {
                this.ItemType = RecentItemType.Folder;
            }

            RemoveCommand = new DelegateCommand(() => OnRemove?.Invoke(), !IsSaving);

            OpenCommand = new DelegateCommand(() =>
            {
                ServiceProvider.LaunchFile(new ProcessStartInfo(FilePath));
            }, !IsSaving);

            PrintCommand = new DelegateCommand(() =>
            {
                ServiceProvider.LaunchFile(new ProcessStartInfo(FilePath) { Verb = "Print" });
            }, CanPrint);

            EncodeCommand = new DelegateCommand(() =>
            {
            }, CanEncode);

            DeleteCommand = new DelegateCommand(() =>
            {
                if (!ServiceProvider.Messenger.ShowYesNo($"Are you sure you want to Delete: {FileName}?", "Confirm Deletion"))
                    return;

                try
                {
                    File.Delete(FilePath);

                    // Remove from List
                    OnRemove?.Invoke();
                }
                catch (Exception E)
                {
                    ServiceProvider.Messenger.ShowError($"Could not Delete file: {FilePath}\n\n\n{E}");
                }
            }, !IsSaving);
        }

        bool CanPrint => !IsSaving && ItemType == RecentItemType.Image;

        bool CanEncode => !IsSaving && ItemType == RecentItemType.Folder;

        public string FilePath { get; }

        public string FileName { get; }

        public RecentItemType ItemType { get; }

        bool _saving;

        public bool IsSaving
        {
            get { return _saving; }
            private set
            {
                _saving = value;

                OnPropertyChanged();
            }
        }

        public void Saved()
        {
            if (IsSaving == false)
                return;

            IsSaving = false;

            RemoveCommand.RaiseCanExecuteChanged(true);
            OpenCommand.RaiseCanExecuteChanged(true);
            DeleteCommand.RaiseCanExecuteChanged(true);

            PrintCommand.RaiseCanExecuteChanged(CanPrint);
            EncodeCommand.RaiseCanExecuteChanged(CanEncode);
        }

        public DelegateCommand RemoveCommand { get; }

        public DelegateCommand OpenCommand { get; }

        public DelegateCommand PrintCommand { get; }

        public DelegateCommand EncodeCommand { get; }

        public DelegateCommand DeleteCommand { get; }

        public event Action OnRemove;
    }
}