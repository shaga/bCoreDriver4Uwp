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
    class MotorSetting : BindableBase
    {
        #region field

        private bool _isShow = true;

        private bool _isFlip;

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

        #endregion

        #region method

        public void Init()
        {
            OnPropertyChanged(nameof(IsShow));
            OnPropertyChanged(nameof(IsFlip));
        }

        #endregion
    }
}
