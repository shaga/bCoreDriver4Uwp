using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Foundation;

namespace LibBcore
{
    public class UnpairingBcoreScanner
    {
        #region property

        public bool IsScanning => Watcher.Status == DeviceWatcherStatus.Started;

        public bool CanStop => !IsScanning && Watcher.Status != DeviceWatcherStatus.Stopping;

        private DeviceWatcher Watcher { get; }

        #endregion

        #region event

        public event EventHandler<DeviceInformation> FoundBcore;
        public event EventHandler<string> RemovedBcore;
        public event EventHandler StoppedScan;  

        #endregion

        #region constructor

        public UnpairingBcoreScanner()
        {
            Watcher = DeviceInformation.CreateWatcher(BluetoothLEDevice.GetDeviceSelectorFromPairingState(false));

            Watcher.Added += OnDeviceAdded;
            Watcher.Removed += OnDeviceRemoved;
            Watcher.Updated += OnDeviceUpdated;
            Watcher.EnumerationCompleted += OnWatcherEnumerationCompleted;
            Watcher.Stopped += OnWatcherStopped;
        }

        #endregion

        #region method

        public void StartScan()
        {
            if (IsScanning) return;

            Watcher.Start();
        }

        public void StopScan()
        {
            if (!IsScanning) return;

            if (Watcher.Status == DeviceWatcherStatus.Stopping) return;

            Watcher.Stop();
        }

        private void OnDeviceAdded(DeviceWatcher watcher, DeviceInformation device)
        {
            FoundBcore?.Invoke(this, device);
        }

        private void OnDeviceRemoved(DeviceWatcher watcher, DeviceInformationUpdate update)
        {
            RemovedBcore?.Invoke(this, update.Id);
        }

        private void OnDeviceUpdated(DeviceWatcher watcher, DeviceInformationUpdate update)
        {
        }

        private void OnWatcherEnumerationCompleted(DeviceWatcher watcher, object parameter)
        {
            Watcher.Stop();
        }

        private void OnWatcherStopped(DeviceWatcher watcher, object parameter)
        {
            StoppedScan(this, EventArgs.Empty);
        }

        #endregion
    }
}
