using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace IGrill.Core
{
    class TemperatureService
    {
        private readonly Guid TEMPERATURE_SERVICE_GUID = Guid.Parse("a5c50000-f186-4bd6-97f2-7ebacba0d708");
        private readonly Guid TEMPERATURE_SERVICE_MINI_GUID = Guid.Parse("63c70000-4a82-4261-95ff-92cf32477861");

        private readonly Guid PROBE_1_TEMPERATURE_CHARACTERISITC_GUID = Guid.Parse("06ef0002-2e06-4b79-9e33-fce2c42805ec");
        private readonly Guid PROBE_2_TEMPERATURE_CHARACTERISITC_GUID = Guid.Parse("06ef0004-2e06-4b79-9e33-fce2c42805ec");
        private readonly Guid PROBE_3_TEMPERATURE_CHARACTERISITC_GUID = Guid.Parse("06ef0006-2e06-4b79-9e33-fce2c42805ec");
        private readonly Guid PROBE_4_TEMPERATURE_CHARACTERISITC_GUID = Guid.Parse("06ef0008-2e06-4b79-9e33-fce2c42805ec");

        public event EventHandler<TemperatureChangedEventArg> TemperatureChanged;

        public List<Guid> Probes { get; internal set; } = new List<Guid>();

        private Guid serviceGuid;

        public TemperatureService(IGrillVersion iGrillVersion)
        {

            switch (iGrillVersion)
            {
                case IGrillVersion.IGrillMini:
                    Probes.Add(PROBE_1_TEMPERATURE_CHARACTERISITC_GUID);
                    serviceGuid = TEMPERATURE_SERVICE_MINI_GUID;
                    break;
                case IGrillVersion.IGrill2:
                case IGrillVersion.IGrill3:
                    Probes.Add(PROBE_1_TEMPERATURE_CHARACTERISITC_GUID);
                    Probes.Add(PROBE_2_TEMPERATURE_CHARACTERISITC_GUID);
                    Probes.Add(PROBE_3_TEMPERATURE_CHARACTERISITC_GUID);
                    Probes.Add(PROBE_4_TEMPERATURE_CHARACTERISITC_GUID);
                    serviceGuid = TEMPERATURE_SERVICE_GUID;
                    break;
                default:
                    throw new NotSupportedException("Not yet supported");
            }

        }

        public async Task RegisterForTemperatureChanges(BluetoothLEDevice bluetoothLeDevice)
        {

            var service = await bluetoothLeDevice.GetGattServiceForUuidAsync(serviceGuid);
            var accessStatus = await service.RequestAccessAsync();
            var openStatus = await service.OpenAsync(GattSharingMode.Exclusive);

            foreach (var characteristics in await service.GetCharacteristics2Async())
            {
                if (!Probes.Contains(characteristics.Uuid))
                {
                    continue;
                }
                Debug.WriteLine("Registering probe " + characteristics.Uuid);
                await characteristics.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                var byteArray = await characteristics.ReadBytesAsync();
                TemperatureChanged?.Invoke(this, new TemperatureChangedEventArg(
                    Probes.IndexOf(characteristics.Uuid), CalculateTemperature(byteArray)));

                characteristics.ValueChanged += (GattCharacteristic sender, GattValueChangedEventArgs args) =>
                {
                    var temperature = ReadTemperature(args.CharacteristicValue);
                    TemperatureChanged?.Invoke(this, new TemperatureChangedEventArg(
                        Probes.IndexOf(characteristics.Uuid), CalculateTemperature(temperature)));
                };
            }
        }

        private int? CalculateTemperature(byte[] bytes)
        {
            var tempInCelsius = bytes[0] + bytes[1] * 265;
            if (tempInCelsius == 65768)
            {
                return null;
            }
            return tempInCelsius;
        }

        private static byte[] ReadTemperature(IBuffer buffer)
        {
            var reader = DataReader.FromBuffer(buffer);
            reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            reader.ByteOrder = ByteOrder.LittleEndian;
            var byteArray = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(byteArray);
            return byteArray;
        }

    }
}
