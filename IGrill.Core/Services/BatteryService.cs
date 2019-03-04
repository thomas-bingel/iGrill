using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace IGrill.Core
{
    class BatteryService
    {
        private readonly Guid BATTERY_SERVICE_GUID = Guid.Parse("0000180f-0000-1000-8000-00805F9B34FB");
        private readonly Guid BATTERY_CHARACTERISTIC = Guid.Parse("00002a19-0000-1000-8000-00805f9b34fb");

        public event EventHandler<int> BatteryLevelChanged; 

        public async Task RegisterForBatteryChanges(BluetoothLEDevice bluetoothLeDevice)
        {
            var services = await bluetoothLeDevice.GetGattServiceForUuidAsync(BATTERY_SERVICE_GUID);
            var characteristics = await services.GetCharacteristicForUuid2Async(BATTERY_CHARACTERISTIC);
            var bytes = await characteristics.ReadBytesAsync();
            BatteryLevelChanged?.Invoke(this, bytes[0]);

            characteristics.ValueChanged += (GattCharacteristic sender, GattValueChangedEventArgs args) =>
            {
                var reader = DataReader.FromBuffer(args.CharacteristicValue);
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                reader.ByteOrder = ByteOrder.LittleEndian;
                var byteArray = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(byteArray);
                Debug.WriteLine("Battery Level: " + byteArray[0]);
                BatteryLevelChanged?.Invoke(this, byteArray[0]);
            };
        }

    }
}
