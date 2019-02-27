using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;


namespace IGrillLibrary
{
    public class IGrill
    {

        private Random rnd = new Random();

        private Guid TEMPERATURE_SERVICE_GUID = Guid.Parse("a5c50000-f186-4bd6-97f2-7ebacba0d708");
        private Guid PROBE_1_TEMPERATURE_CHARACTERISITC_GUID = Guid.Parse("06ef0002-2e06-4b79-9e33-fce2c42805ec");
        private Guid PROBE_2_TEMPERATURE_CHARACTERISITC_GUID = Guid.Parse("06ef0004-2e06-4b79-9e33-fce2c42805ec");
        private Guid PROBE_3_TEMPERATURE_CHARACTERISITC_GUID = Guid.Parse("06ef0006-2e06-4b79-9e33-fce2c42805ec");
        private Guid PROBE_4_TEMPERATURE_CHARACTERISITC_GUID = Guid.Parse("06ef0008-2e06-4b79-9e33-fce2c42805ec");

        private Guid DEVICE_SERVICE_GUID = Guid.Parse("64AC0000-4A4B-4B58-9F37-94D3C52FFDF7");
        private Guid FIRMWARE_CHARACTERISTIC_GUID = Guid.Parse("64AC0001-4A4B-4B58-9F37-94D3C52FFDF7");
        private Guid APP_CHALLENGE_GUID = Guid.Parse("64AC0002-4A4B-4B58-9F37-94D3C52FFDF7");
        private Guid DEVICE_CHALLENGE_GUID = Guid.Parse("64AC0003-4A4B-4B58-9F37-94D3C52FFDF7");
        private Guid DEVICE_RESPONSE_GUID = Guid.Parse("64AC0004-4A4B-4B58-9F37-94D3C52FFDF7");

        private Guid BATTERY_SERVICE_GUID = Guid.Parse("0000180f-0000-1000-8000-00805F9B34FB");

        private byte[] iGrill2Key = new byte[] {
            0xdf, 0x33, 0xe0, 0x89, 0xf4, 0x48, 0x4e, 0x73,
            0x92, 0xd4, 0xcf, 0xb9, 0x46, 0xe7, 0x85, 0xb6 };

        Dictionary<Guid, EventHandler<int>> probes = new Dictionary<Guid, EventHandler<int>>();

        private BluetoothLEDevice bluetoothLeDevice;

        public event EventHandler<int> OnProbe1TemperatureChange;
        public event EventHandler<int> OnProbe2TemperatureChange;
        public event EventHandler<int> OnProbe3TemperatureChange;
        public event EventHandler<int> OnProbe4TemperatureChange;

        public async Task InitAsync(String deviceId)
        {
            bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceId);


            if (bluetoothLeDevice == null)
            {
                throw new Exception("iGrill not found");
            }

            bluetoothLeDevice.ConnectionStatusChanged += async (BluetoothLEDevice device, object obj) =>
            {
                Debug.WriteLine("Connection status changed to: " + device.ConnectionStatus);
            };

            await Authenticate();
            await RegisterForTemperatureChanges();
            await RegisterForBatteryChanges();
        }


        private async Task RegisterForTemperatureChanges()
        {
            probes.Add(PROBE_1_TEMPERATURE_CHARACTERISITC_GUID, OnProbe1TemperatureChange);
            probes.Add(PROBE_2_TEMPERATURE_CHARACTERISITC_GUID, OnProbe2TemperatureChange);
            probes.Add(PROBE_3_TEMPERATURE_CHARACTERISITC_GUID, OnProbe3TemperatureChange);
            probes.Add(PROBE_4_TEMPERATURE_CHARACTERISITC_GUID, OnProbe4TemperatureChange);


            var service = await bluetoothLeDevice.GetGattServiceForUuidAsync(TEMPERATURE_SERVICE_GUID);
            var accessStatus = await service.RequestAccessAsync();
            var openStatus = await service.OpenAsync(GattSharingMode.Exclusive);

            foreach (var characteristics in await service.GetCharacteristics2Async())
            {
                if (probes.ContainsKey(characteristics.Uuid))
                {
                    Debug.WriteLine("Registering probe " + characteristics.Uuid);
                    await characteristics.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    characteristics.ValueChanged += (GattCharacteristic sender, GattValueChangedEventArgs args) =>
                    {
                        var eventHandler = probes.GetValueOrDefault(characteristics.Uuid);
                        var temperature = ReadTemperature(args);
                        if (temperature != 65768)
                        {
                            eventHandler?.Invoke(this, temperature);
                        }
                    };
                }
            }
        }

        private async Task Authenticate()
        {
            Debug.WriteLine("Start authentication with iGrill");

            // Gett Service
            var service = await bluetoothLeDevice.GetGattServiceForUuidAsync(DEVICE_SERVICE_GUID);

            var characteristicsResult = await service.GetCharacteristicsAsync();
            var characteristics = characteristicsResult.Characteristics;

            var challengeCharacteristic = characteristics.First((c) => c.Uuid == APP_CHALLENGE_GUID);
            var responseCharacterisitc = characteristics.First((c) => c.Uuid == DEVICE_RESPONSE_GUID);
            var deviceChallengeCharacteristig = characteristics.First((c) => c.Uuid == DEVICE_CHALLENGE_GUID);

            // send app challenge
            Debug.WriteLine("Send chalange to iGrill");
            var challenge = new byte[16];
            Array.Copy(Enumerable.Range(0, 8).Select(n => (byte)rnd.Next(0, 255)).ToArray(), challenge, 8);
            await challengeCharacteristic.WriteBytesAsync(challenge);

            // read device challenge
            Debug.WriteLine("Read encrypted challenge from iGrill");
            byte[] encrypted_device_challenge = await deviceChallengeCharacteristig.ReadBytesAsync();
            var device_challenge = Encryption.Decrypt(encrypted_device_challenge, iGrill2Key);

            // verify device challenge
            Debug.WriteLine("Comparing challenges...");
            for (int i = 0; i < 8; i++)
            {
                if (challenge[i] != device_challenge[i])
                    throw new Exception("Invalid device challange");
            }

            // send device response
            Debug.WriteLine("Send encrypted response to iGrill");
            var device_response = new byte[16];
            Array.Copy(device_challenge, 8, device_response, 8, 8);

            var encrypted_device_response = Encryption.Encrypt(device_response, iGrill2Key);
            await responseCharacterisitc.WriteBytesAsync(encrypted_device_response);

            // Authenticated
            Debug.WriteLine("Authentication with iGrill successful");
        }

        private async Task<GattCharacteristic> FindCharacteristic(Guid serviceGuid, Guid characteristicGuid)
        {
            var services = await bluetoothLeDevice.GetGattServicesForUuidAsync(serviceGuid);
            var characteristics = await services.Services.First().GetCharacteristicsForUuidAsync(characteristicGuid);
            return characteristics.Characteristics.First();
        }

        private static int ReadTemperature(GattValueChangedEventArgs args)
        {
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            reader.ByteOrder = ByteOrder.LittleEndian;
            var byteArray = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(byteArray);
            var tempInCelsius = byteArray[0] + byteArray[1] * 265;
            return tempInCelsius;
        }



        private async Task RegisterForBatteryChanges()
        {

            //var batteryService = await bluetoothLeDevice.GetGattServicesForUuidAsync(BATTERY_SERVICE_GUID);
            //var batteryCharacteristic = FindCharacteristic(BATTERY_SERVICE_GUID, BATT;
            //batteryCharacteristic.Characteristics.First().ValueChanged += (GattCharacteristic sender,
            //    GattValueChangedEventArgs args) =>
            //{
            //    var reader = DataReader.FromBuffer(args.CharacteristicValue);
            //    reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            //    reader.ByteOrder = ByteOrder.LittleEndian;
            //    var byteArray = new byte[reader.UnconsumedBufferLength];
            //    reader.ReadBytes(byteArray);
            //    Debug.WriteLine("Battery Level: " + byteArray[0]);
            //};


        }


    }
}
