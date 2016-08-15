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

        #region property

        public BcoreManager Manager { get; private set; }
        public BcoreSettings Settings { get; private set; }

        #region motor

        private DateTime[] UpdatedMotorValue =
        {
            DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue
        };

        public int MotorValue1
        {
            get { return _motorValue[0]; }
            set { SetProperty(ref _motorValue[0], value); }
        }

        public int MotorValue2
        {
            get { return _motorValue[1]; }
            set { SetProperty(ref _motorValue[1], value); }
        }

        public int MotorValue3
        {
            get { return _motorValue[2]; }
            set { SetProperty(ref _motorValue[2], value); }
        }

        public int MotorValue4
        {
            get { return _motorValue[3]; }
            set { SetProperty(ref _motorValue[3], value); }
        }

        #endregion

        #region servo

        private DateTime[] UpdatedServoValue =
        {
            DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue
        };

        public int ServoValue1
        {
            get { return _servoValue[0]; }
            set { SetProperty(ref _servoValue[0], value); }
        }

        public int ServoValue2
        {
            get { return _servoValue[1]; }
            set { SetProperty(ref _servoValue[1], value); }
        }

        public int ServoValue3
        {
            get { return _servoValue[2]; }
            set { SetProperty(ref _servoValue[2], value); }
        }

        public int ServoValue4
        {
            get { return _servoValue[3]; }
            set { SetProperty(ref _servoValue[3], value); }
        }

        #endregion

        #region PortOut

        public bool PortOutValue1
        {
            get { return _portOutValue[0]; }
            set { SetProperty(ref _portOutValue[0], value); }
        }

        public bool PortOutValue2
        {
            get { return _portOutValue[1]; }
            set { SetProperty(ref _portOutValue[1], value); }
        }

        public bool PortOutValue3
        {
            get { return _portOutValue[2]; }
            set { SetProperty(ref _portOutValue[2], value); }
        }

        public bool PortOutValue4
        {
            get { return _portOutValue[3]; }
            set { SetProperty(ref _portOutValue[3], value); }
        }

        #endregion
        
        #endregion

        public BcoreInfo(DeviceInformation info)
        {
            Manager = new BcoreManager(info);
        }

        public void Dispose()
        {
            if (Settings != null)
            {
                foreach (var setting in Settings.ServoSettings)
                {
                    setting.PropertyChanged -= OnServoSettingPropertyChanged;
                }
            }

            Manager?.Dispose();
        }

        public async Task<bool> Init()
        {
            Settings = await BcoreSettings.Load(Manager.DeviceName);

            return await Manager.Init();
        }

        public void WriteMotorValue(int idx, bool isForce = false)
        {
            var setting = Settings.MotorSettings[idx];

            var now = DateTime.Now;

            var span = now - UpdatedMotorValue[idx];

            if (!isForce || span.TotalMilliseconds < 30) return;

            UpdatedMotorValue[idx] = now;

            Manager.WriteMotorPwm(idx, _motorValue[idx], setting.IsFlip);
        }

        public void WriteServoValue(int idx, bool isForce = false)
        {
            var setting = Settings.ServoSettings[idx];

            var now = DateTime.Now;

            var span = now - UpdatedServoValue[idx];

            if (!isForce || span.TotalMilliseconds < 30) return;

            UpdatedServoValue[idx] = now;

            Manager.WriteServoPos(idx, _servoValue[idx], setting.IsFlip, setting.Trim);

            if (idx > 0 || Settings.ServoSettings.Skip(1).All(s => !s.IsSync)) return;

            for (var i = 1; i < 4; i++)
            {
                if (!Settings.ServoSettings[i].IsShow) continue;

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

            return (Manager?.FuntcionInfo?.IsMotorPortEnable(idx) ?? false) && (Settings.MotorSettings[idx].IsShow);
        }

        public bool IsVisibleServo(int idx)
        {
            if (idx < 0 || Bcore.MaxFunctionCount <= idx) return false;

            return (Manager?.FuntcionInfo?.IsServoPortEnable(idx) ?? false) && (Settings.ServoSettings[idx].IsShow);
        }

        public bool IsVisiblePortOut(int idx)
        {
            if (idx < 0 || Bcore.MaxFunctionCount <= idx) return false;

            return (Manager?.FuntcionInfo?.IsPortOutEnable(idx) ?? false) && (Settings.PortOutSettings[idx].IsShow);
        }

        private void OnServoSettingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var setting = sender as ServoSetting;

            if (setting == null) return;

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
    }
}
