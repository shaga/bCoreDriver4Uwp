using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace LibBcore
{
    public class BcoreFoundEventArgs : EventArgs
    {
        public DeviceInformation Info { get; }

        public string Name => Info.Name;

        public BcoreFoundEventArgs(DeviceInformation info)
        {
            Info = info;
        }
    }

    public class BcoreRemovedEventArgs : EventArgs
    {
        public string Id { get; }

        public BcoreRemovedEventArgs(string id)
        {
            Id = id;
        }
    }
}
