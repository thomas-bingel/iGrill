using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;

namespace IGrillLibrary
{
    class GenericService
    {
        private readonly Guid GENERIC_SERVICE_GUID = Guid.Parse("00001800-0000-1000-8000-00805f9b34fb");

        public async Task<string> GetDeviceNameAsync(BluetoothLEDevice bluetoothLeDevice)
        {
            var service = await bluetoothLeDevice.GetGattServiceForUuidAsync(GENERIC_SERVICE_GUID);
            var characteristics = await service.GetCharacteristicForHandleAsync(21);
            return await characteristics.ReadStringAsync();
        }

        public async Task<string> GetAppearanceNameAsync(BluetoothLEDevice bluetoothLeDevice)
        {
            var service = await bluetoothLeDevice.GetGattServiceForUuidAsync(GENERIC_SERVICE_GUID);
            var characteristics = await service.GetCharacteristicForHandleAsync(23);
            return await characteristics.ReadStringAsync();
        }

    }
}
