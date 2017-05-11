using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using LibBcore;
using Prism.Mvvm;

namespace bCoreDriver.Models
{
    class BcoreInfo : BindableBase, IDisposable
    {
        #region field

        private readonly int[] _motorValue =
        {
            Bcore.StopMotorPwm, Bcore.StopMotorPwm,
            Bcore.StopMotorPwm, Bcore.StopMotorPwm
        };

        private readonly int[] _servoValue =
        {
            Bcore.CenterServoPos, Bcore.CenterServoPos, Bcore.CenterServoPos, Bcore.CenterServoPos
        };

        private readonly bool[] _portOutValue =
        {
            false, false, false, false
        };

        private bool _isConnected;

        #endregion

        #region property

        public BcoreManager Manager { get; private set; }
        public BcoreSettings Settings { get; private set; }

        public BcoreFunctionInfo FunctionInfo => Manager?.FunctionInfo;

        public string DeviceName => Manager?.DeviceName;

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                SetProperty(ref _isConnected, value);
                OnPropertyChanged(nameof(IsConnecting));
            }
        }

        public bool IsConnecting => !_isConnected;

        #region motor

        private DateTime[] UpdatedMotorValue { get; } =
            {
                DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue
            };

        public int MotorValue1
        {
            get { return _motorValue[0]; }
            set
            {
                SetProperty(ref _motorValue[0], value); 
                WriteMotorValue(0);
            }
        }

        public int MotorValue2
        {
            get { return _motorValue[1]; }
            set
            {
                SetProperty(ref _motorValue[1], value); 
                WriteMotorValue(1);
            }
        }

        public int MotorValue3
        {
            get { return _motorValue[2]; }
            set
            {
                SetProperty(ref _motorValue[2], value); 
                WriteMotorValue(2);
            }
        }

        public int MotorValue4
        {
            get { return _motorValue[3]; }
            set
            {
                SetProperty(ref _motorValue[3], value); 
                WriteMotorValue(3);
            }
        }

        #endregion

        #region servo

        private DateTime[] UpdatedServoValue { get; } =
        {
            DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue
        };

        public int ServoValue1
        {
            get { return _servoValue[0]; }
            set
            {
                SetProperty(ref _servoValue[0], value); 
                WriteServoValue(0);
            }
        }

        public int ServoValue2
        {
            get { return _servoValue[1]; }
            set
            {
                SetProperty(ref _servoValue[1], value); 
                WriteServoValue(1);
            }
        }

        public int ServoValue3
        {
            get { return _servoValue[2]; }
            set
            {
                SetProperty(ref _servoValue[2], value); 
                WriteServoValue(2);
            }
        }

        public int ServoValue4
        {
            get { return _servoValue[3]; }
            set
            {
                SetProperty(ref _servoValue[3], value); 
                WriteServoValue(3);
            }
        }

        #endregion

        #region PortOut

        public bool PortOutValue1
        {
            get { return _portOutValue[0]; }
            set
            {
                SetProperty(ref _portOutValue[0], value); 
                WritePortOutValue(0);
            }
        }

        public bool PortOutValue2
        {
            get { return _portOutValue[1]; }
            set
            {
                SetProperty(ref _portOutValue[1], value); 
                WritePortOutValue(1);
            }
        }

        public bool PortOutValue3
        {
            get { return _portOutValue[2]; }
            set
            {
                SetProperty(ref _portOutValue[2], value); 
                WritePortOutValue(2);
            }
        }

        public bool PortOutValue4
        {
            get { return _portOutValue[3]; }
            set
            {
                SetProperty(ref _portOutValue[3], value); 
                WritePortOutValue(3);
            }
        }

        #endregion

        #endregion

        #region event

        public event EventHandler<bool> ConnectionChanged;

        #endregion

        #region constructor

        public BcoreInfo()
        {
        }

        #endregion

        #region method

        public void Dispose()
        {
            if (Settings != null)
            {
                foreach (var setting in Settings.ServoSettings)
                {
                    setting.PropertyChanged -= OnServoSettingPropertyChanged;
                }
            }

            if (Manager != null) Manager.ConnectionChanged -= OnBcoreConnectionChanged;
            Manager?.Dispose();
        }

        public async Task<bool> Init(DeviceInformation info)
        {
            var result = true;

            if (Manager == null || Manager.DeviceName != info.Name)
            {
                if (Manager != null)
                {
                    Manager.ConnectionChanged -= OnBcoreConnectionChanged;
                }
                Manager?.Dispose();
                Manager = new BcoreManager(info);
                Manager.ConnectionChanged += OnBcoreConnectionChanged;
            }

            if (!Manager.IsInitialized)
            {
                result = await Manager.Init();
            }

            if (!result) return result;

            if (Settings == null || Settings.BcoreName != info.Name)
            {
                Settings = await BcoreSettings.Load(DeviceName);
                Settings.Init();
                foreach (var setting in Settings.ServoSettings)
                {
                    setting.PropertyChanged += OnServoSettingPropertyChanged;
                }
            }

            IsConnected = true;

            return result;
        }

        public async Task<int> ReadBatteryVoltage()
        {
            if (Manager == null || !Manager.IsInitialized) return 0;

            return await Manager.ReadBattery();
        }

        public void WriteMotorValue(int idx, bool isForce = false)
        {
            var setting = Settings.MotorSettings[idx];

            var now = DateTime.Now;

            var span = now - UpdatedMotorValue[idx];

            if (!isForce && span.TotalMilliseconds < 30) return;

            UpdatedMotorValue[idx] = now;

            Manager.WriteMotorPwm(idx, _motorValue[idx], setting.IsFlip);
        }

        public void WriteServoValue(int idx, bool isForce = false)
        {
            var setting = Settings.ServoSettings[idx];

            var now = DateTime.Now;

            var span = now - UpdatedServoValue[idx];

            if (!isForce && span.TotalMilliseconds < 30) return;

            UpdatedServoValue[idx] = now;

            Manager.WriteServoPos(idx, _servoValue[idx], setting.IsFlip, setting.Trim);

            if (idx > 0 || Settings.ServoSettings.Skip(1).All(s => !s.IsSync)) return;

            for (var i = 1; i < 4; i++)
            {
                if (!Settings.ServoSettings[i].IsSync) continue;

                WriteServoValue(i, _servoValue[idx]);
            }
        }

        public void WriteServoValue(int idx, int value)
        {
            var setting = Settings.ServoSettings[idx];

            Manager.WriteServoPos(idx, value, setting.IsFlip, setting.Trim);
        }

        public void WritePortOutValue(int idx)
        {
            Manager.WritePortOutState(idx, _portOutValue[idx]);
        }

        public bool IsVisibleMotor(int idx)
        {
            if (idx < 0 || Bcore.MaxFunctionCount <= idx) return false;

            return (FunctionInfo?.IsMotorPortEnable(idx) ?? false) && (Settings.MotorSettings[idx].IsShow);
        }

        public bool IsVisibleServo(int idx)
        {
            if (idx < 0 || Bcore.MaxFunctionCount <= idx) return false;

            return (FunctionInfo?.IsServoPortEnable(idx) ?? false) && (Settings.ServoSettings[idx].IsShow);
        }

        public bool IsVisiblePortOut(int idx)
        {
            if (idx < 0 || Bcore.MaxFunctionCount <= idx) return false;

            return (FunctionInfo?.IsPortOutEnable(idx) ?? false) && (Settings.PortOutSettings[idx].IsShow);
        }

        private void OnServoSettingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var setting = sender as ServoSetting;

            if (setting == null || !setting.IsInited) return;

            var idx = Settings.ServoSettings.IndexOf(setting);

            if (idx < 0) return;

            if (idx == 0 || !setting.IsSync)
            {
                WriteServoValue(idx, true);
            }
            else
            {
                WriteServoValue(idx, _servoValue[0]);
            }
        }

        private void OnBcoreConnectionChanged(object sender, bool isConnected)
        {
            ConnectionChanged?.Invoke(this, isConnected);
        }

        #endregion
    }
}
