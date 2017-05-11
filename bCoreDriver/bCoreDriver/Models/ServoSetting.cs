using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Prism.Mvvm;

namespace bCoreDriver.Models
{
    [JsonObject(MemberSerialization.OptOut)]
    class ServoSetting : BindableBase
    {
        #region field

        private bool _isShow = true;

        private bool _isFlip;

        private bool _isSync;

        private int _trim;

        #endregion

        #region property

        public bool IsShow
        {
            get { return _isShow; }
            set { SetProperty(ref _isShow, value); }
        }

        public bool IsFlip
        {
            get { return _isFlip; }
            set { SetProperty(ref _isFlip, value); }
        }

        public bool IsSync
        {
            get { return _isSync; }
            set
            {
                SetProperty(ref _isSync, value); 
                OnPropertyChanged(nameof(IsEnable));
            }
        }

        [JsonIgnore]
        public bool IsEnable => !IsSync;

        public int Trim
        {
            get { return _trim; }
            set { SetProperty(ref _trim, value); }
        }

        [JsonIgnore]
        public bool IsInited { get; private set; }

        #endregion

        #region method

        public void Init()
        {
            OnPropertyChanged(nameof(IsShow));
            OnPropertyChanged(nameof(IsFlip));
            OnPropertyChanged(nameof(IsSync));
            OnPropertyChanged(nameof(Trim));
            IsInited = true;
        }

        #endregion
    }
}
