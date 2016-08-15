using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace LibBcore
{
    static class BcoreUuid
    {
        public static readonly Guid BcoreService = Guid.Parse("389CAAF0-843F-4D3B-959D-C954CCE14655");
        public static string BcoreServiceSelector => GattDeviceService.GetDeviceSelectorFromUuid(BcoreService);

        public static readonly Guid BatteryCharacteristic = Guid.Parse("389CAAF1-843F-4D3B-959D-C954CCE14655");
        public static readonly Guid MotorCharacteristic = Guid.Parse("389CAAF2-843F-4D3B-959D-C954CCE14655");
        public static readonly Guid PortOutCharacteristic = Guid.Parse("389CAAF3-843F-4D3B-959D-C954CCE14655");
        public static readonly Guid ServoCharacteristic = Guid.Parse("389CAAF4-843F-4D3B-959D-C954CCE14655");

        public static readonly Guid FunctionCharacteristic = Guid.Parse("389CAAFF-843F-4D3B-959D-C954CCE14655");
    }
}
