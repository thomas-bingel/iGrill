using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Devices.Radios;
using Windows.Storage.Streams;


namespace IGrillLibrary
{
    public class IGrill
    {
        private Random rnd = new Random();

        private readonly Guid TEMPERATURE_SERVICE_GUID = Guid.Parse("a5c50000-f186-4bd6-97f2-7ebacba0d708");
// iGrill Mini        private readonly Guid TEMPERATURE_SERVICE_GUID = Guid.Parse("63c70000-4a82-4261-95ff-92cf32477861");
        private readonly Guid PROBE_1_TEMPERATURE_CHARACTERISITC_GUID = Guid.Parse("06ef0002-2e06-4b79-9e33-fce2c42805ec");
        private readonly Guid PROBE_2_TEMPERATURE_CHARACTERISITC_GUID = Guid.Parse("06ef0004-2e06-4b79-9e33-fce2c42805ec");
        private readonly Guid PROBE_3_TEMPERATURE_CHARACTERISITC_GUID = Guid.Parse("06ef0006-2e06-4b79-9e33-fce2c42805ec");
        private readonly Guid PROBE_4_TEMPERATURE_CHARACTERISITC_GUID = Guid.Parse("06ef0008-2e06-4b79-9e33-fce2c42805ec");

        private readonly Guid DEVICE_SERVICE_GUID = Guid.Parse("64AC0000-4A4B-4B58-9F37-94D3C52FFDF7");

        private readonly Guid FIRMWARE_CHARACTERISTIC_GUID = Guid.Parse("64AC0001-4A4B-4B58-9F37-94D3C52FFDF7");
        private readonly Guid APP_CHALLENGE_GUID = Guid.Parse("64AC0002-4A4B-4B58-9F37-94D3C52FFDF7");
        private readonly Guid DEVICE_CHALLENGE_GUID = Guid.Parse("64AC0003-4A4B-4B58-9F37-94D3C52FFDF7");
        private readonly Guid DEVICE_RESPONSE_GUID = Guid.Parse("64AC0004-4A4B-4B58-9F37-94D3C52FFDF7");

        private readonly Guid BATTERY_SERVICE_GUID = Guid.Parse("0000180f-0000-1000-8000-00805F9B34FB");
        private readonly Guid BATTERY_CHARACTERISTIC_GUID = Guid.Parse("00002A19-0000-1000-8000-00805F9B34FB");

        // iGrill Mini
        private readonly byte[] iGrillMiniKey = new byte[] {
            0xED, 0x5E, 0x30, 0x8E, 0x8B, 0xCC, 0x91, 0x13,
            0x30, 0x6C, 0xD4, 0x68, 0x54, 0x15, 0x3E, 0xDD  };

        // iGrill v2
        private readonly byte[] iGrill2Key = new byte[] {
            0xDF, 0x33, 0xE0, 0x89, 0xF4, 0x48, 0x4E, 0x73,
            0x92, 0xD4, 0xCF, 0xB9, 0x46, 0xE7, 0x85, 0xB6  };

        // iGrill v3
        private readonly byte[] iGrill3Key = new byte[]
        {
            0x27, 0x62, 0xFC, 0x5E, 0xCA, 0x13, 0x45, 0xE5,
            0x9D, 0x11, 0xDE, 0xB6, 0xF6, 0xF3, 0x8C, 0x1C
        };
        /*
            encryption_key = [39, 98, -4, 94, -54, 19, 69, -27, -99, 17, -34, -74, -10, -13, -116, 28] - iGrill V3
            encryption_key = [-33, 51, -32, -119, -12, 72, 78, 115, -110, -44, -49, -71, 70, -25, -123, -74] - iGrill V2 
            encryption_key = [-19, 94, 48, -114, -117, -52, -111, 19, 48, 108, -44, 104, 84, 21, 62, -35] - iGrill Mini 
         */

        List<Guid> probes = new List<Guid>();

        private BluetoothLEDevice bluetoothLeDevice;
        private readonly IGrillVersion iGrillVersion;
        private readonly Timer simulationTimer;
        private String deviceId;

        public IGrill(IGrillVersion iGrillVersion, string deviceId)
            : this(iGrillVersion)
        {
            this.deviceId = deviceId;
        }

        public IGrill(IGrillVersion iGrillVersion)
        {
            this.iGrillVersion = iGrillVersion;
            switch(iGrillVersion)
            {
                case IGrillVersion.IGrillMini:
                    probes.Add(PROBE_1_TEMPERATURE_CHARACTERISITC_GUID);
                    break;
                case IGrillVersion.IGrill2:
                case IGrillVersion.IGrill3:
                    probes.Add(PROBE_1_TEMPERATURE_CHARACTERISITC_GUID);
                    probes.Add(PROBE_2_TEMPERATURE_CHARACTERISITC_GUID);
                    probes.Add(PROBE_3_TEMPERATURE_CHARACTERISITC_GUID);
                    probes.Add(PROBE_4_TEMPERATURE_CHARACTERISITC_GUID);
                    break;
                case IGrillVersion.Simulation:
                    probes.Add(PROBE_1_TEMPERATURE_CHARACTERISITC_GUID);
                    simulationTimer = new System.Timers.Timer(1000);
                    simulationTimer.Elapsed += (sender, arg) =>
                    {
                        OnTemperatureChanged?.Invoke(this, new TemperatureChangedEventArg(0, rnd.Next()));
                    };
                    simulationTimer.AutoReset = true;
                    break;
                default:
                    throw new NotSupportedException("Not yet supported");
            }
        }

        public static IGrill FromDeviceInformation(DeviceInformation device)
        {
            if (device.Name.StartsWith("iGrill_mini"))
            {
                return new IGrillLibrary.IGrill(IGrillVersion.IGrillMini, device.Id);
            }
            else if (device.Name.StartsWith("iGrill_V2"))
            {
                return new IGrillLibrary.IGrill(IGrillVersion.IGrill2, device.Id);
            }
            else if (device.Name.StartsWith("iGrill_V3"))
            {
                return new IGrillLibrary.IGrill(IGrillVersion.IGrill3, device.Id);
            }
            else
            {
                throw new Exception("Unknown device with name " + device.Name);
            }
        }

        public int ProbeCount { get { return probes.Count; } }

        public event EventHandler<TemperatureChangedEventArg> OnTemperatureChanged;

        public async Task ConnectAsync()
        {
            if (this.iGrillVersion == IGrillVersion.Simulation)
            {
                simulationTimer.Enabled = true;
                return;
            }
            bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceId);
            
            if (bluetoothLeDevice == null)
            {
                throw new Exception("iGrill not found");
            }

            bluetoothLeDevice.ConnectionStatusChanged += (BluetoothLEDevice device, object obj) =>
            {
                Debug.WriteLine("Connection status changed to: " + device.ConnectionStatus);
            };

            await Authenticate();
            await RegisterForTemperatureChanges();
            //await RegisterForBatteryChanges();
        }


        private async Task RegisterForTemperatureChanges()
        {

            var service = await bluetoothLeDevice.GetGattServiceForUuidAsync(TEMPERATURE_SERVICE_GUID);
            var accessStatus = await service.RequestAccessAsync();
            var openStatus = await service.OpenAsync(GattSharingMode.Exclusive);

            foreach (var characteristics in await service.GetCharacteristics2Async())
            {
                if (!probes.Contains(characteristics.Uuid))
                {
                    continue;
                }
                Debug.WriteLine("Registering probe " + characteristics.Uuid);
                await characteristics.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                characteristics.ValueChanged += (GattCharacteristic sender, GattValueChangedEventArgs args) =>
                {
                    var temperature = ReadTemperature(args);
                    if (temperature != 65768)
                    {
                        var index = probes.IndexOf(characteristics.Uuid);
                        OnTemperatureChanged?.Invoke(this, new TemperatureChangedEventArg(index, temperature));
                    }
                };
            }
        }

        private async Task Authenticate()
        {
            Debug.WriteLine("Start authentication with iGrill");

            // Gett Service
            var service = await bluetoothLeDevice.GetGattServiceForUuidAsync(DEVICE_SERVICE_GUID);

            var characteristics = await service.GetCharacteristics2Async();
            var challengeCharacteristic = characteristics.First((c) => c.Uuid == APP_CHALLENGE_GUID);
            var deviceChallengeCharacteristig = characteristics.First((c) => c.Uuid == DEVICE_CHALLENGE_GUID);
            var responseCharacterisitc = characteristics.First((c) => c.Uuid == DEVICE_RESPONSE_GUID);

            Debug.WriteLine("Send challenge to iGrill");
            var challenge = new byte[16];
            Array.Copy(Enumerable.Range(0, 8).Select(n => (byte)rnd.Next(0, 255)).ToArray(), challenge, 8);
            await challengeCharacteristic.WriteBytesAsync(challenge);

            // read device challenge
            Debug.WriteLine("Read encrypted challenge from iGrill");
            byte[] encrypted_device_challenge = await deviceChallengeCharacteristig.ReadBytesAsync();
            var device_challenge = Encryption.Decrypt(encrypted_device_challenge, GetEncryptionKey(iGrillVersion));

            // verify device challenge
            Debug.WriteLine("Comparing challenges...");
            Enumerable.Range(0, 8).ToList().ForEach(i => {
                if (challenge[i] != device_challenge[i])
                    throw new Exception("Invalid device challange");
            });

            // send device response
            Debug.WriteLine("Send encrypted response to iGrill");
            var device_response = new byte[16];
            Array.Copy(device_challenge, 8, device_response, 8, 8);

            var encrypted_device_response = Encryption.Encrypt(device_response, iGrill2Key);
            await responseCharacterisitc.WriteBytesAsync(encrypted_device_response);

            // Authenticated
            Debug.WriteLine("Authentication with iGrill successful");
        }

        private byte[] GetEncryptionKey(IGrillVersion iGrillVersion)
        {
            switch (iGrillVersion)
            {
                case IGrillVersion.IGrillMini:
                    return iGrillMiniKey;
                case IGrillVersion.IGrill2:
                    return iGrill2Key;
                case IGrillVersion.IGrill3:
                    return iGrill3Key;
                default:
                    throw new Exception(String.Format("No key configured for iGrill Version {0}", iGrillVersion));
            }
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
            var services = await bluetoothLeDevice.GetGattServiceForUuidAsync(BATTERY_SERVICE_GUID);
            var characteristics = await services.GetCharacteristics2Async();
   
            characteristics.First().ValueChanged += (GattCharacteristic sender, GattValueChangedEventArgs args) =>
            {
                var reader = DataReader.FromBuffer(args.CharacteristicValue);
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                reader.ByteOrder = ByteOrder.LittleEndian;
                var byteArray = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(byteArray);
                Debug.WriteLine("Battery Level: " + byteArray[0]);
            };
        }


        public static async Task<bool> IsBluetoothEnabledAsync()
        {
            var radios = await Radio.GetRadiosAsync();
            var bluetoothRadio = radios.FirstOrDefault(radio => radio.Kind == RadioKind.Bluetooth);
            return bluetoothRadio != null && bluetoothRadio.State == RadioState.On;
        }

        public static async Task<bool> IsBluetoothSupportedAsync()
        {
            var radios = await Radio.GetRadiosAsync();
            return radios.FirstOrDefault(radio => radio.Kind == RadioKind.Bluetooth) != null;
        }
    }
}
