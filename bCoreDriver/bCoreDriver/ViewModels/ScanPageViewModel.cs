using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using bCoreDriver.Models;
using bCoreDriver.Views;
using LibBcore;
using Prism.Commands;
using Prism.Mvvm;

namespace bCoreDriver.ViewModels
{
    class ScanPageViewModel : BindableBase
    {
        #region const

        private static readonly TimeSpan ScanTimeoutSpan = TimeSpan.FromSeconds(30);

        #endregion

        #region inner class

        public class BcoreInfo
        {
            public DeviceInformation Info { get; }

            public string BcoreId => Info.Id;

            public string Name { get; }

            public BcoreInfo(DeviceInformation info, BcoreSettings settings)
            {
                Info = info;
                Name = settings.DisplayName;
            }
        }

        #endregion

        #region field

        private bool _isScanning;
        private BcoreInfo _selectedInfo;

        private DelegateCommand _commandScan;
        private DelegateCommand _commandMovePairingPage;

        #endregion

        #region property

        public bool IsScanning
        {
            get { return _isScanning; }
            set { SetProperty(ref _isScanning, value); }
        }

        public BcoreInfo SelectedInfo
        {
            get { return _selectedInfo; }
            set
            {
                SetProperty(ref _selectedInfo, value);
                if (_selectedInfo == null) return;

                MovePage(typeof(ControlPage), _selectedInfo.Info);
            }
        }

        public ObservableCollection<BcoreInfo> FoundBcores { get; set; } = new ObservableCollection<BcoreInfo>();

        private BcoreScanner Scanner { get; }

        private static CoreDispatcher Dispathcer => CoreApplication.MainView.CoreWindow.Dispatcher;

        private static Frame AppFrame => Window.Current.Content as Frame;

        private DispatcherTimer ScanTimeoutTimer { get; set; }

        #region command

        public DelegateCommand CommandScan => _commandScan ?? (_commandScan = new DelegateCommand(Scan));

        public DelegateCommand CommandMovePairingPage
            =>
                _commandMovePairingPage ??
                (_commandMovePairingPage = new DelegateCommand(() => MovePage(typeof(PairingPage))));

        #endregion

        #endregion

        #region constructor

        public ScanPageViewModel()
        {
            Scanner = new BcoreScanner();
            Scanner.FoundBcore += OnFoundBcore;

            ScanTimeoutTimer = new DispatcherTimer();
            ScanTimeoutTimer.Interval = ScanTimeoutSpan;
            ScanTimeoutTimer.Tick += (s, e) =>
            {
                ScanStop();
            };
        }

        #endregion

        #region method

        private async void OnFoundBcore(object sender, BcoreFoundEventArgs e)
        {
            await Dispathcer.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                if (FoundBcores.Any(i => i.BcoreId == e.Info.Id)) return;

                var settings = await BcoreSettings.Load(e.Info.Name);
                FoundBcores.Add(new BcoreInfo(e.Info, settings));
            });
        }

        private void Scan()
        {
            if (IsScanning)
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
            if (IsScanning) return;

            FoundBcores.Clear();
            Scanner.StartScan();
            IsScanning = true;
            ScanTimeoutTimer.Start();
        }

        private void ScanStop()
        {
            if (!IsScanning) return;

            IsScanning = false;
            if (ScanTimeoutTimer.IsEnabled) ScanTimeoutTimer.Stop();
            Scanner.StopScan();
        }

        private void MovePage(Type page)
        {
            if (IsScanning) Scanner.StopScan();

            AppFrame.Navigate(page);
        }

        private void MovePage(Type page, object parameter)
        {
            if (IsScanning) Scanner.StopScan();

            AppFrame.Navigate(page, parameter);
        }

        #endregion
    }
}
