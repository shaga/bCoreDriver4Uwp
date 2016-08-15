using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
    class ControlPageViewModel : BindableBase
    {
        #region const

        private const int ReadBatteryIntervalSec = 1;

        #endregion

        #region field

        private int _batteryVoltage;

        private int _countLeftSlider = 1;
        private int _countRightSlider = 1;

        private readonly int[] _motorValue = {128, 128, 128, 128};
        private readonly int[] _servoValue = {128, 128, 128, 128};
        private readonly bool[] _portOutValue = {false, false, false, false};

        private DelegateCommand _commandOpenSetting;
        private DelegateCommand<BcoreSliderParam> _commandValueUpdated;

        #endregion

        #region property

        private static CoreDispatcher Dispatcher => CoreApplication.MainView.Dispatcher;

        private static Frame AppFrame => Window.Current.Content as Frame;

        public BcoreInfo Info { get; set; }

        private BcoreManager Manager { get; set; }

        private DispatcherTimer ReadBatteryTimer { get; set; }

        public int BatteryVoltage
        {
            get { return _batteryVoltage; }
            set { SetProperty(ref _batteryVoltage, value); }
        }

        private BcoreFunctionInfo FunctionInfo { get; set; }

        private BcoreSettings Settings { get; set; }

        #region motor

        public int CountLeftSlider
        {
            get { return _countLeftSlider; }
            set { SetProperty(ref _countLeftSlider, value); }
        }

        public int CountRightSlider
        {
            get { return _countRightSlider; }
            set { SetProperty(ref _countRightSlider, value); }
        }
        public bool IsVisibleMotor1 => IsVisibleMotor();
        public bool IsVisibleMotor2 => IsVisibleMotor();
        public bool IsVisibleMotor3 => IsVisibleMotor();
        public bool IsVisibleMotor4 => IsVisibleMotor();

        #endregion

        #region servo

        public bool IsVisibleServo1 => IsVisibleServo();
        public bool IsVisibleServo2 => IsVisibleServo();
        public bool IsVisibleServo3 => IsVisibleServo();
        public bool IsVisibleServo4 => IsVisibleServo();

        #endregion

        #region port out

        public bool IsVisiblePortOut1 => IsVisiblePortOut();
        public bool IsVisiblePortOut2 => IsVisiblePortOut();
        public bool IsVisiblePortOut3 => IsVisiblePortOut();
        public bool IsVisiblePortOut4 => IsVisiblePortOut();

        #endregion

        #region command

        public ICommand CommandOpenSetting
            =>
                _commandOpenSetting ??
                (_commandOpenSetting = new DelegateCommand(() => AppFrame.Navigate(typeof(BcoreSettingPage), Manager)));

        public DelegateCommand<BcoreSliderParam> CommandValueUpdated =>
            _commandValueUpdated ??
            (_commandValueUpdated = new DelegateCommand<BcoreSliderParam>(OnSliderValueUpdated));

        #endregion

        #endregion

        #region method

        public async void Init(DeviceInformation info)
        {
            if (Manager == null || Manager.DeviceName != info.Name)
            {
                Manager?.Dispose();
                Manager = new BcoreManager(info);
            }

            var result = await Manager.Init();

            if (!result)
            {
                AppFrame.GoBack();
                return;
            }

            FunctionInfo = Manager.FuntcionInfo;

            InitVisibility();

            BatteryVoltage = await Manager.ReadBattery();

            ReadBatteryTimer = new DispatcherTimer();
            ReadBatteryTimer.Interval = TimeSpan.FromSeconds(ReadBatteryIntervalSec);
            ReadBatteryTimer.Tick += OnReadBattery;

            ReadBatteryTimer.Start();
        }

        public void Finish()
        {
            if (ReadBatteryTimer?.IsEnabled ?? false) ReadBatteryTimer.Stop();
            Manager.Dispose();
        }

        private async void OnReadBattery(object sender, object parameter)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                BatteryVoltage = await Manager.ReadBattery();
            });
        }

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        private async void InitVisibility()
        {
            if (FunctionInfo == null) return;

            if (FunctionInfo.IsMotorPortEnable(0) && FunctionInfo.IsMotorPortEnable(2))
                CountLeftSlider = 2;
            else if (FunctionInfo.IsMotorPortEnable(1) && FunctionInfo.IsMotorPortEnable(3))
                CountLeftSlider = 1;
            else
                CountLeftSlider = 0;

            Settings = await BcoreSettings.Load(Manager.DeviceName);

            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(IsVisibleMotor1));
                OnPropertyChanged(nameof(IsVisibleMotor2));
                OnPropertyChanged(nameof(IsVisibleMotor3));
                OnPropertyChanged(nameof(IsVisibleMotor4));

                OnPropertyChanged(nameof(IsVisibleServo1));
                OnPropertyChanged(nameof(IsVisibleServo2));
                OnPropertyChanged(nameof(IsVisibleServo3));
                OnPropertyChanged(nameof(IsVisibleServo4));

                OnPropertyChanged(nameof(IsVisiblePortOut1));
                OnPropertyChanged(nameof(IsVisiblePortOut2));
                OnPropertyChanged(nameof(IsVisiblePortOut3));
                OnPropertyChanged(nameof(IsVisiblePortOut4));
            });
        }

        private bool IsVisibleMotor([CallerMemberName] string name = null)
        {
            var idx = GetPortNum(name) - 1;
            return Info.IsVisibleMotor(idx);
        }

        private bool IsVisibleServo([CallerMemberName] string name = null)
        {
            var idx = GetPortNum(name) - 1;
            return Info.IsVisibleServo(idx);
        }

        private bool IsVisiblePortOut([CallerMemberName] string name = null)
        {
            var idx = GetPortNum(name) - 1;
            return Info.IsVisiblePortOut(idx);
        }

        private int GetPortNum(string name, [CallerMemberName]string prefix = null)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(prefix)) return 0;

            var numStr = name.Substring(prefix.Length);

            int num;

            if (!int.TryParse(numStr, out num)) return 0;

            return num;
        }

        private void OnSliderValueUpdated(BcoreSliderParam param)
        {
            if (param.Type == ESliderType.Motor)
            {
                Info.WriteMotorValue(param.Idx, true);
            }
            else if (param.Type == ESliderType.Servo)
            {
                Info.WriteServoValue(param.Idx, true);
            }
        }

        #endregion
    }
}
