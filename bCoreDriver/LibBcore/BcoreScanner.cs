using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;

namespace LibBcore
{
    public class BcoreScanner
    {
        #region property

        public bool IsScanning => AdvertisementWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started;

        private BluetoothLEAdvertisementWatcher AdvertisementWatcher { get; } = new BluetoothLEAdvertisementWatcher();

        private DeviceInformationCollection PairedBcores { get; set; }

        #endregion

        #region event

        public event EventHandler<BcoreFoundEventArgs> FoundBcore;
        public event EventHandler FinishedScan;

        #endregion

        #region constructor

        public BcoreScanner()
        {
            AdvertisementWatcher.ScanningMode = BluetoothLEScanningMode.Active;
            AdvertisementWatcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(BcoreUuid.BcoreService);
            AdvertisementWatcher.Received += OnAdvertisemntReceived;
            AdvertisementWatcher.Stopped += (w, e) => FinishedScan?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region method

        #region public

        public async void StartScan()
        {
            await GetPairedBcore();

            AdvertisementWatcher.Start();
        }

        public void StopScan()
        {
            AdvertisementWatcher.Stop();
        }

        #endregion

        #region private

        private async Task GetPairedBcore()
        {
            PairedBcores = await DeviceInformation.FindAllAsync(BcoreUuid.BcoreServiceSelector);
        }

        private void OnAdvertisemntReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs e)
        {
            var address = $"_{e.BluetoothAddress.ToString("x12")}";

            var bcore = PairedBcores?.FirstOrDefault(i => i.Id.Contains(address));

            if (bcore == null) return;

            FoundBcore?.Invoke(this, new BcoreFoundEventArgs(bcore));
        }

        #endregion

        #endregion
    }
}
