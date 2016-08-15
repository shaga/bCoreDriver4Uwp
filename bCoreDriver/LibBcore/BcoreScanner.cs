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

        private DeviceWatcher DeviceWatcher { get; }

        private DeviceInformationCollection PairedBcores { get; set; }

        #endregion

        #region event

        public event EventHandler<BcoreFoundEventArgs> FoundBcore;
        public event EventHandler<BcoreRemovedEventArgs> RemovedBcore;
        public event EventHandler FinishedScan;

        #endregion

        #region constructor

        public BcoreScanner()
        {
            AdvertisementWatcher.ScanningMode = BluetoothLEScanningMode.Active;
            AdvertisementWatcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(BcoreUuid.BcoreService);
            AdvertisementWatcher.Received += OnAdvertisemntReceived;
            AdvertisementWatcher.Stopped += (w, e) => FinishedScan?.Invoke(this, EventArgs.Empty);

            DeviceWatcher = DeviceInformation.CreateWatcher(BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
                null);
            DeviceWatcher.Added += OnDeviceWatcherAdded;
            DeviceWatcher.Removed += OnDeviceWatherRemoved;
        }

        #endregion

        #region method

        #region public

        public async void StartScan(bool withUnpaired)
        {
            await GetPairedBcore();

            AdvertisementWatcher.Start();

            if (withUnpaired)
            {
                DeviceWatcher.Start();
            }
        }

        public void StopScan()
        {
            AdvertisementWatcher.Stop();

            if (DeviceWatcher.Status == DeviceWatcherStatus.Started ||
                DeviceWatcher.Status == DeviceWatcherStatus.EnumerationCompleted)
            {
                DeviceWatcher.Stop();
            }
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

        private void OnDeviceWatcherAdded(DeviceWatcher watcher, DeviceInformation info)
        {
            if (!info.Name.StartsWith("bCore")) return;

            FoundBcore?.Invoke(this, new BcoreFoundEventArgs(info));
        }

        private void OnDeviceWatherRemoved(DeviceWatcher watcher, DeviceInformationUpdate update)
        {
            RemovedBcore?.Invoke(this, new BcoreRemovedEventArgs(update.Id));
        }

        #endregion

        #endregion
    }
}
