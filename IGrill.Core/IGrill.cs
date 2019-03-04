using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Radios;


namespace IGrill.Core
{
    public class IGrill
    {
        private BluetoothLEDevice bluetoothLeDevice;
        private readonly IGrillVersion iGrillVersion;
        private readonly String deviceId;

        public String DeviceName { get; internal set; }
        public String FirmwareVersion { get; internal set; }
        public int BatteryLevel { get; internal set; }
        public int ProbeCount { get { return temperatureService.Probes.Count; } }

        private readonly GenericService genericService;
        private readonly AuthenticationService authenticationService;
        private readonly TemperatureService temperatureService;
        private readonly BatteryService batteryService;

        public event EventHandler<TemperatureChangedEventArg> TemperatureChanged;
        public event EventHandler<int> BatteryLevelChanges;
        public event EventHandler<BluetoothConnectionStatus> ConnectionStatusChanged;

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
            batteryService = new BatteryService();
        }

        public async Task ConnectAsync()
        {
            if (!await IsBluetoothEnabledAsync()) {
                throw new Exception("Bluetooth is not enabled.");
            }

            if (!await IsBluetoothSupportedAsync())
            {
                throw new Exception("Bluetooth is not supportet.");
            }

            bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceId);
            
            if (bluetoothLeDevice == null)
            {
                throw new Exception("iGrill not found");
            }

            // Event for connection status changed
            bluetoothLeDevice.ConnectionStatusChanged += (BluetoothLEDevice device, object obj) =>
            {
                Debug.WriteLine("Connection status changed to: " + device.ConnectionStatus);
                ConnectionStatusChanged?.Invoke(device, device.ConnectionStatus);
            };

            // Read basic device information like name, etc.
            this.DeviceName = await genericService.GetDeviceNameAsync(bluetoothLeDevice);
            this.FirmwareVersion = await genericService.GetFirmwareVersionAsync(bluetoothLeDevice);

            // Read battery level and register for updates
            batteryService.BatteryLevelChanged += (sender, level) =>
            {
                this.BatteryLevelChanges?.Invoke(sender, level);
            };
            await batteryService.RegisterForBatteryChanges(bluetoothLeDevice);

            // Authenticate iGrill to read probes
            await authenticationService.Authenticate(bluetoothLeDevice);

            // Read probes and register for updates 
            temperatureService.TemperatureChanged += (sender, args) =>
            {
                this.TemperatureChanged?.Invoke(sender, args);
            };
            await temperatureService.RegisterForTemperatureChanges(bluetoothLeDevice);
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
