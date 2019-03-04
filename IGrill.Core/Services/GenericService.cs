using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;

namespace IGrill.Core
{
    class GenericService
    {
        private static readonly Guid DEVICE_SERVICE_GUID = Guid.Parse("64AC0000-4A4B-4B58-9F37-94D3C52FFDF7");
        private static readonly Guid GENERIC_SERVICE_GUID = Guid.Parse("00001800-0000-1000-8000-00805f9b34fb");
        private static readonly Guid FIRMWARE_CHARACTERISTIC_GUID = Guid.Parse("64AC0001-4A4B-4B58-9F37-94D3C52FFDF7");


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

        public async Task<string> GetFirmwareVersionAsync(BluetoothLEDevice bluetoothLeDevice)
        {
            var service = await bluetoothLeDevice.GetGattServiceForUuidAsync(DEVICE_SERVICE_GUID);
            var characteristics = await service.GetCharacteristicForUuid2Async(FIRMWARE_CHARACTERISTIC_GUID);
            return await characteristics.ReadStringAsync();
        }

    }
}
