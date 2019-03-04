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

       
        private readonly Guid DEVICE_NAME_CHARACTERISTIC_GUID = Guid.Parse("00001800-0000-1000-8000-00805f9b34fb");


        private BluetoothLEDevice bluetoothLeDevice;
        private readonly IGrillVersion iGrillVersion;

        private String deviceId;
        public String DeviceName { get; internal set; }
        public String FirmwareVersion { get; internal set; }
        public int BatteryLevel { get; internal set; }

        public int ProbeCount { get { return temperatureService.Probes.Count; } }

        private readonly GenericService genericService;
        private readonly AuthenticationService authenticationService;
        private readonly TemperatureService temperatureService;

        public event EventHandler<TemperatureChangedEventArg> TemperatureChanged;
        public event EventHandler<int> BatteryLevelChanges;



        public IGrill(IGrillVersion iGrillVersion, string deviceId)
            : this(iGrillVersion)
        {
            this.deviceId = deviceId;
           
        }

        public IGrill(IGrillVersion iGrillVersion)
        {
            this.iGrillVersion = iGrillVersion;
            authenticationService = new AuthenticationService(iGrillVersion);
            temperatureService = new TemperatureService(iGrillVersion);
            genericService = new GenericService();
        }

        public async Task ConnectAsync()
        {
            //if (this.iGrillVersion == IGrillVersion.Simulation)
            //{
            //    simulationTimer.Enabled = true;
            //    return;
            //}
            bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceId);
            
            if (bluetoothLeDevice == null)
            {
                throw new Exception("iGrill not found");
            }

            bluetoothLeDevice.ConnectionStatusChanged += (BluetoothLEDevice device, object obj) =>
            {
                Debug.WriteLine("Connection status changed to: " + device.ConnectionStatus);
            };

            var batteryService = new BatteryService(bluetoothLeDevice);

            // Read basic device information like name, etc.
            this.DeviceName = await genericService.GetDeviceNameAsync(bluetoothLeDevice);
            this.FirmwareVersion = await GetFirmwareVersionAsync();

            // Read battery level and register for updates
            batteryService.BatteryLevelChanged += (sender, level) =>
            {
                this.BatteryLevelChanges?.Invoke(sender, level);
            };
            await batteryService.RegisterForBatteryChanges();

            // Authenticate iGrill to read probes
            await authenticationService.Authenticate(bluetoothLeDevice);

            // Read probes and register for updates 
            temperatureService.TemperatureChanged += (sender, args) =>
            {
                this.TemperatureChanged?.Invoke(sender, args);
            };
            await temperatureService.RegisterForTemperatureChanges(bluetoothLeDevice);

       
        }

        public async Task<string> GetFirmwareVersionAsync()
        {
            var service = await bluetoothLeDevice.GetGattServiceForUuidAsync(IGrillGuids.DEVICE_SERVICE_GUID);
            var characteristics = await service.GetCharacteristicForUuid2Async(IGrillGuids.FIRMWARE_CHARACTERISTIC_GUID);
            return await characteristics.ReadStringAsync();
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
