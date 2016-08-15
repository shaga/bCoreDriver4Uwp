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
    class PortOutSetting : BindableBase
    {
        #region field

        private bool _isShow = true;

        #endregion

        #region property

        public bool IsShow
        {
            get { return _isShow; }
            set { SetProperty(ref _isShow, value); }
        }

        #endregion
    }
}
