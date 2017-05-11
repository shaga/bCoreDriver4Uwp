using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using bCoreDriver.Models;
using LibBcore;
using Prism.Mvvm;

namespace bCoreDriver.ViewModels
{
    class BcoreSettingPageViewModel : BindableBase
    {
        #region property

        public string BcoreName
        {
            get { return Settings.BcoreName; }
            set
            {
                Settings.BcoreName = value;
                OnPropertyChanged();
            }
        }

        private string DeviceName { get; set; }

        private BcoreInfo Info { get; set; }

        private BcoreFunctionInfo FunctionInfo => Info?.FunctionInfo;

        public BcoreSettings Settings => Info?.Settings;

        #region visibility

        #region motor

        public Visibility VisibilityMotor1 => GetVisiblity(IsMotorEnable, 0);
        public Visibility VisibilityMotor2 => GetVisiblity(IsMotorEnable, 1);
        public Visibility VisibilityMotor3 => GetVisiblity(IsMotorEnable, 2);
        public Visibility VisibilityMotor4 => GetVisiblity(IsMotorEnable, 3);

        #endregion

        #region servo

        public Visibility VisibilityServo1 => GetVisiblity(IsServoEnable, 0);
        public Visibility VisibilityServo2 => GetVisiblity(IsServoEnable, 1);
        public Visibility VisibilityServo3 => GetVisiblity(IsServoEnable, 2);
        public Visibility VisibilityServo4 => GetVisiblity(IsServoEnable, 3);

        #endregion

        #region port out

        public Visibility VisibilityPortOut1 => GetVisiblity(IsPortOutEnable, 0);
        public Visibility VisibilityPortOut2 => GetVisiblity(IsPortOutEnable, 1);
        public Visibility VisibilityPortOut3 => GetVisiblity(IsPortOutEnable, 2);
        public Visibility VisibilityPortOut4 => GetVisiblity(IsPortOutEnable, 3);

        #endregion

        #endregion

        #region motor settings

        #endregion

        #endregion

        #region method

        public void Init(BcoreInfo info)
        {
            Info = info;

            OnPropertyChanged(nameof(Settings));
            Settings.Init();
        }

        public async void Finish()
        {
            await BcoreSettings.Save(Settings);
        }

        private bool IsMotorEnable(int idx)
        {
            return FunctionInfo?.IsMotorPortEnable(idx) ?? false;
        }

        private bool IsServoEnable(int idx)
        {
            return FunctionInfo?.IsServoPortEnable(idx) ?? false;
        }

        private bool IsPortOutEnable(int idx)
        {
            return FunctionInfo?.IsPortOutEnable(idx) ?? false;
        }

        private static Visibility GetVisiblity(Func<int, bool> function, int idx)
        {
            return (function != null && function(idx)) ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion
    }
}
