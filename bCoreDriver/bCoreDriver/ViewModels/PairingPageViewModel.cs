using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using LibBcore;
using Prism.Commands;
using Prism.Mvvm;

namespace bCoreDriver.ViewModels
{
    class PairingPageViewModel : BindableBase
    {
        #region field

        private bool _isScanning;
        private DeviceInformation _selectedBcore;

        private DelegateCommand _commandScan;
        private DelegateCommand _commandBack;

        #endregion

        #region property

        public bool IsScanning
        {
            get { return _isScanning; }
            set { SetProperty(ref _isScanning, value); }
        }

        public DeviceInformation SelectedBcore
        {
            get { return _selectedBcore; }
            set
            {
                SetProperty(ref _selectedBcore, value);
                if (_selectedBcore == null) return;
                PairingBcore(_selectedBcore);
            }
        }

        public ObservableCollection<DeviceInformation> FoundBcores { get; }

        private UnpairingBcoreScanner Scanner { get; }

        private static Frame AppFrame => Window.Current.Content as Frame;

        private static CoreDispatcher Dispatcher => CoreApplication.MainView.Dispatcher;

        #region command

        public ICommand CommandScan => _commandScan ?? (_commandScan = new DelegateCommand(Scan));

        public ICommand CommandBack => _commandBack ?? (_commandBack = new DelegateCommand(Back));

        #endregion

        #endregion

        #region constructor

        public PairingPageViewModel()
        {
            FoundBcores = new ObservableCollection<DeviceInformation>();
            Scanner = new UnpairingBcoreScanner();
            Scanner.FoundBcore += OnFoundUnpairBcore;
            Scanner.RemovedBcore += OnRemovedUnpairBcore;
            Scanner.StoppedScan += OnStoppedScan;
        }

        #endregion

        #region method

        private void Back()
        {
            if (AppFrame?.CanGoBack ?? false)
                AppFrame.GoBack();
        }

        private void Scan()
        {
            if (Scanner.IsScanning)
            {
                ScanStop();
            }
            else
            {
                ScanStart();
            }
        }

        private void ScanStart()
        {
            Scanner.StartScan();
            IsScanning = true;
        }

        private void ScanStop()
        {
            Scanner.StopScan();
        }

        private void OnFoundUnpairBcore(object sender, DeviceInformation bcore)
        {
            if (FoundBcores.Any(i => i.Id == bcore.Id)) return;

            RunOnUiThread(() =>
            {
                FoundBcores.Add(bcore);
            });
        }

        private void OnRemovedUnpairBcore(object sender, string id)
        {
            var bcore = FoundBcores.FirstOrDefault(i => i.Id == id);

            if (bcore == null) return;
            
            RunOnUiThread(() =>
            {
                FoundBcores.Remove(bcore);
            });
        }

        private void OnStoppedScan(object sender, EventArgs e)
        {
           RunOnUiThread(() => IsScanning = false);
        }

        private async void RunOnUiThread(Action action)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => action());
        }

        private async void PairingBcore(DeviceInformation bcore)
        {
            if (bcore == null || bcore.Pairing.IsPaired || !bcore.Pairing.CanPair)
            {
                return;
            }

            if (IsScanning) ScanStop();

            var result = await bcore.Pairing.PairAsync();

            if (result.Status == DevicePairingResultStatus.PairingCanceled) return;

            if (result.Status == DevicePairingResultStatus.Paired)
            {
                SelectedBcore = null;
                RunOnUiThread(async () =>
                {
                    FoundBcores.Remove(bcore);

                    var dialog = new ContentDialog
                    {
                        Title = "bCore Driver",
                        Content = $"Please restart bCore \"{bcore.Name}\", if you use it.",
                        PrimaryButtonText = "OK"
                    };
                    await dialog.ShowAsync();
                });
            }
        }

        #endregion
    }
}
