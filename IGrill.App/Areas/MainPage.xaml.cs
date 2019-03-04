using IGrill.Playground;
using IGrillLibrary;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace IGrill.App.Areas
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {


        public MainPageViewModel ViewModel
        {
            get { return (MainPageViewModel)this.DataContext; }
        }

        private readonly MqttService mqttService;
        private IGrillLibrary.IGrill igrill;
        private DevicePicker devicePicker = null;

        public MainPage()
        {
            this.DataContext = new MainPageViewModel();
            mqttService = new MqttService();


            devicePicker = new DevicePicker();
            this.devicePicker.DeviceSelected += async (devicePicker, args) =>
            {
                var device = args.SelectedDevice;
                devicePicker.Hide();

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    //await mqttService.StartAsync();


                    await PairDeviceIfNecessary(device);
                    await ConnectIGrill(device);
                    Settings.SelectedDeviceId = device.Id;
                });

            };
            devicePicker.Filter.SupportedDeviceSelectors.Add(BluetoothLEDevice.GetDeviceSelectorFromPairingState(false));
            devicePicker.Filter.SupportedDeviceSelectors.Add(BluetoothLEDevice.GetDeviceSelectorFromPairingState(true));

            this.InitializeComponent();

        }

        private async Task ConnectIGrill(DeviceInformation device)
        {
            
            igrill = IGrillLibrary.IGrillFactory.FromDeviceInformation(device);

            foreach (int i in Enumerable.Range(0, igrill.ProbeCount))
            {
                ViewModel.Probes.Add(new ProbeViewModel(i));
            }

            igrill.TemperatureChanged += async (sender, args) =>
            {
                Debug.WriteLine(String.Format("{0}: Probe {1} = {2}°C", DateTime.Now, args.ProbeIndex, args.Temperature));

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ViewModel.Probes[args.ProbeIndex].Value = args.Temperature;
                });
            };
            igrill.BatteryLevelChanges += async (sender, batteryLevel) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ViewModel.BatteryLevel = batteryLevel;
                });
            };

            // MQTT
            igrill.TemperatureChanged += async (object sender, TemperatureChangedEventArg args) =>
            {
                await mqttService.SendProbeTemperatureAsync(args.ProbeIndex, args.Temperature);
            };
            igrill.BatteryLevelChanges += async (sender, batteryLevel) =>
            {
                await mqttService.SendBatteryLevelAsync(batteryLevel);
            };

            await igrill.ConnectAsync();
            ViewModel.Name = igrill.DeviceName;
            ViewModel.FirmwareVersion = igrill.FirmwareVersion;

        }

        public void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            devicePicker.Show(new Rect(0, 0, 200, 500), Windows.UI.Popups.Placement.Below);
        }

        private static async Task PairDeviceIfNecessary(DeviceInformation device)
        {
            if (device.Pairing.IsPaired)
            {
                return;
            }
            else
            {
                if (device.Pairing.CanPair)
                {
                    var customPairing = device.Pairing.Custom;
                    customPairing.PairingRequested += (sender, pairingRequestArgs) =>
                    {
                        pairingRequestArgs.Accept("00000");
                    };

                    var result = await customPairing.PairAsync(DevicePairingKinds.ConfirmOnly);
                    if ((result.Status == DevicePairingResultStatus.Paired) ||
                        (result.Status == DevicePairingResultStatus.AlreadyPaired))
                    {
                        return;
                    } else
                    {
                        throw new Exception(String.Format("Could not pair device. Pairing status: {0}", result.Status));
                    }
                }
                throw new Exception("Pairing ot supported by device.");

            }
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {

            //for (int i=0; i<4; i++)
            //{
            //    mqttService.SendProbeTemperatureAsync(i, null);
            //}
            //mqttService.SendBatteryLevelAsync(null);
        }
    }
}
