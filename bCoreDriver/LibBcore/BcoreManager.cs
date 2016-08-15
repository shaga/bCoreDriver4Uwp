using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;

namespace LibBcore
{
    public class BcoreManager : IDisposable
    {
        #region const

        #endregion

        #region property

        public bool IsInitialized { get; private set; }

        public string DeviceName => BcoreInfo?.Name;

        public BcoreFunctionInfo FuntcionInfo { get; private set;  }

        private DeviceInformation BcoreInfo { get; }

        private GattDeviceService BcoreService { get; set; }

        private Dictionary<Guid, GattCharacteristic> BcoreCharacteristics { get; set; }

        private bool[] PortOutState { get; } = new bool[Bcore.MaxFunctionCount];

        #endregion

        #region event

        public event EventHandler<bool> ConnectionChanged;

        #endregion

        #region constructor

        public BcoreManager(DeviceInformation info)
        {
            BcoreInfo = info;
        }

        ~BcoreManager()
        {
            Dispose();
        }

        #endregion

        #region method

        #region public

        public void Dispose()
        {
            BcoreService?.Dispose();
            BcoreService = null;
            IsInitialized = false;
        }

        public async Task<bool> InitWithPairing()
        {
            var tcs = new TaskCompletionSource<GattDeviceService>();

            IsInitialized = false;

            var watcher = DeviceInformation.CreateWatcher(BcoreUuid.BcoreServiceSelector, null);
            watcher.Added += async (w, i) =>
            {
                if (i.Name != BcoreInfo.Name || BcoreService != null || !i.Pairing.IsPaired) return;

                var svc = await GattDeviceService.FromIdAsync(i.Id);
                Debug.WriteLine($"Found SVC:{svc != null}");
                if (svc == null) return;
                tcs.SetResult(svc);
            };

            watcher.Updated += async (w, u) =>
            {
                var device = await DeviceInformation.CreateFromIdAsync(u.Id);
                if (device.Name != BcoreInfo.Name || BcoreService != null || !device.Pairing.IsPaired) return;

                var svc = await GattDeviceService.FromIdAsync(device.Id);
                Debug.WriteLine($"Found SVC:{svc != null}");
                if (svc == null) return;
                tcs.SetResult(svc);
            };

            watcher.Start();

            var result = await BcoreInfo.Pairing.PairAsync();

            if (result.Status != DevicePairingResultStatus.AlreadyPaired &&
                result.Status != DevicePairingResultStatus.Paired)
            {
                tcs.SetResult(null);
            }

            BcoreService = tcs.Task.Result;

            watcher.Stop();

            if (BcoreService == null) return false;

            return true;
        }

        public async Task<bool> Init()
        {
            IsInitialized = false;

            BcoreService = await GattDeviceService.FromIdAsync(BcoreInfo.Id);

            if (BcoreService == null)
            {
                ConnectionChanged?.Invoke(this, false);
                return false;
            }

            BcoreService.Device.ConnectionStatusChanged += OnChangedConnection;

            var characteristics = BcoreService.GetAllCharacteristics();

            BcoreCharacteristics = new Dictionary<Guid, GattCharacteristic>();

            foreach (var characteristic in characteristics)
            {
                BcoreCharacteristics.Add(characteristic.Uuid, characteristic);
            }

            IsInitialized = true;

            FuntcionInfo = await ReadFunctionInfo();

            return true;
        }

        public async Task<int> ReadBattery()
        {
            if (!IsInitialized) return 0;

            var value = await ReadValue(BcoreUuid.BatteryCharacteristic);

            if (value == null) return 0;

            var voltage = value[0] | (value[1] << 8);

            return voltage;
        }

        public async void WriteMotorPwm(int idx, int speed, bool isFlip = false)
        {
            if (!IsInitialized) return;

            if (idx < 0 || Bcore.MaxFunctionCount <= idx) return;

            await WriteValue(BcoreUuid.MotorCharacteristic, Bcore.CreateMotorSpeedValue(idx, speed, isFlip));
        }

        public async Task WriteMotorPwmAsync(int idx, int speed, bool isFlip = false)
        {
            if (!IsInitialized) return;

            if (idx < 0 || Bcore.MaxFunctionCount <= idx) return;

            await WriteValue(BcoreUuid.MotorCharacteristic, Bcore.CreateMotorSpeedValue(idx, speed, isFlip));
        }

        public async void WriteServoPos(int idx, int pos, bool isFlip = false, int trim = 0)
        {
            if (!IsInitialized) return;

            if (idx < 0 || Bcore.MaxFunctionCount <= idx) return;

            await WriteValue(BcoreUuid.ServoCharacteristic, Bcore.CreateServoPosValue(idx, pos, isFlip, trim));
        }

        public async Task WriteServoPosAsync(int idx, int pos, bool isFlip = false, int trim = 0)
        {
            if (!IsInitialized) return;

            if (idx < 0 || Bcore.MaxFunctionCount <= idx) return;

            await WriteValue(BcoreUuid.ServoCharacteristic, Bcore.CreateServoPosValue(idx, pos, isFlip, trim));
        }

        public async void WriteServoPos(int idx, int pos, int trim)
        {
            if (!IsInitialized) return;

            if (idx < 0 || Bcore.MaxFunctionCount <= idx) return;

            await WriteValue(BcoreUuid.ServoCharacteristic, Bcore.CreateServoPosValue(idx, pos, trim));
        }

        public async Task WriteServoPosAsync(int idx, int pos, int trim)
        {
            if (!IsInitialized) return;

            if (idx < 0 || Bcore.MaxFunctionCount <= idx) return;

            await WriteValue(BcoreUuid.ServoCharacteristic, Bcore.CreateServoPosValue(idx, pos, trim));
        }

        public async void WritePortOutState(int idx, bool isOn)
        {
            if (!IsInitialized) return;

            if (idx < 0 || Bcore.MaxFunctionCount <= idx) return;

            PortOutState[idx] = isOn;

            await WriteValue(BcoreUuid.PortOutCharacteristic, Bcore.CreatePortOutValue(PortOutState));
        }

        public async Task WritePortOutStateAsync(int idx, bool isOn)
        {
            if (!IsInitialized) return;

            if (idx < 0 || Bcore.MaxFunctionCount <= idx) return;

            PortOutState[idx] = isOn;

            await WriteValue(BcoreUuid.PortOutCharacteristic, Bcore.CreatePortOutValue(PortOutState));
        }

        public async Task<BcoreFunctionInfo> ReadFunctionInfo()
        {
            if (!IsInitialized) return null;

            var value = await ReadValue(BcoreUuid.FunctionCharacteristic);

            if (value == null) return null;

            return new BcoreFunctionInfo(value);
        }

        #endregion

        #region private

        private void OnChangedConnection(BluetoothLEDevice device, object eventArgs)
        {
            switch (device.ConnectionStatus)
            {
                case BluetoothConnectionStatus.Connected:
                    IsInitialized = true;
                    ConnectionChanged?.Invoke(this, true);
                    break;
                case BluetoothConnectionStatus.Disconnected:
                    IsInitialized = false;
                    BcoreService?.Dispose();
                    BcoreService = null;
                    BcoreCharacteristics.Clear();
                    ConnectionChanged?.Invoke(this, false);
                    break;
            }
        }

        private async Task WriteValue(Guid uuid, byte[] data)
        {
            if (!BcoreCharacteristics.ContainsKey(uuid)) return;

            var characteristic = BcoreCharacteristics[uuid];

            if (!characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse))
                return;

            await characteristic.WriteValueAsync(data.AsBuffer(), GattWriteOption.WriteWithoutResponse);
        }

        private async Task<byte[]> ReadValue(Guid uuid)
        {
            if (!BcoreCharacteristics.ContainsKey(uuid)) return null;

            var characteristic = BcoreCharacteristics[uuid];

            if (!characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read)) return null;

            var result = await characteristic.ReadValueAsync();

            return result.Value.ToArray();
        }

        #endregion

        #endregion
    }
}
