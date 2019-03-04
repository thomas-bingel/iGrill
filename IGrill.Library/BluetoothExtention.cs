using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace IGrillLibrary
{
    internal static class BluetoothExtention
    {
        public static async Task<byte[]> ReadBytesAsync(this GattCharacteristic gatt)
        {
            var result = await gatt.ReadValueAsync(BluetoothCacheMode.Uncached);
            if (result.Status != GattCommunicationStatus.Success)
            {
                throw new Exception("Could not read from Characteristic UUID=" + gatt.Uuid);
            }
            var reader = DataReader.FromBuffer(result.Value);
            reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            reader.ByteOrder = ByteOrder.LittleEndian;
            var data = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(data);
            return data;
        }

        public static async Task<string> ReadStringAsync(this GattCharacteristic gatt)
        {
            var bytes = await ReadBytesAsync(gatt);
            return Encoding.UTF8.GetString(bytes);
        }

        public static async Task<GattDeviceService> GetGattServiceForUuidAsync(this BluetoothLEDevice bluetoothLEDevice, Guid uuid)
        {
            var serviceResult = await bluetoothLEDevice.GetGattServicesForUuidAsync(uuid);
            if (serviceResult.Status != GattCommunicationStatus.Success || serviceResult.Services.Count == 0)
            {
                throw new Exception("Could not find GATT service with UUID=" + uuid.ToString());
            }
            return serviceResult.Services.FirstOrDefault();
        }

        public static async Task<IReadOnlyList<GattCharacteristic>> GetCharacteristics2Async(this GattDeviceService service)
        {
            var result = await service.GetCharacteristicsAsync();
            if (result.Status != GattCommunicationStatus.Success)
            {
                throw new Exception(
                    String.Format("Could not get characteristics from service with UUID={0}", service.Uuid));
            }
            return result.Characteristics;
        }

        

        public static async Task<GattCharacteristic> GetCharacteristicForUuid2Async(this GattDeviceService service, Guid guid)
        {
            var result = await service.GetCharacteristicsForUuidAsync(guid);
            if (result.Status != GattCommunicationStatus.Success)
            {
                throw new Exception(
                    String.Format("Could not get characteristics from service with UUID={0}", guid));
            }
            return result.Characteristics.First();
        }

        public static async Task<GattCharacteristic> GetCharacteristicForHandleAsync(this GattDeviceService service, uint handle)
        {
            var result = await service.GetCharacteristics2Async();
            var ret = result.FirstOrDefault( c => c.AttributeHandle == handle);
            if (ret == null)
            {
                foreach (var c in result)
                {
                    Debug.WriteLine(
                        String.Format("Attribute Handle UUID={0} AttributeHandle={1} ", c.Uuid, c.AttributeHandle));
                }
                throw new Exception(
                  String.Format("Could not get characteristics from handle with ID={0}", handle));
            }
            return ret;
        }

        public static async Task WriteBytesAsync(this GattCharacteristic gatt, byte[] data)
        {
            var writeStatus = await gatt.WriteValueWithResultAsync(data.AsBuffer());
            if (writeStatus.Status != GattCommunicationStatus.Success)
            {
                throw new Exception(
                    String.Format("Could not write data to Characteristic. Status={0} UUID={1}", 
                    writeStatus.Status, gatt.Uuid));
            }
        }
    }
}