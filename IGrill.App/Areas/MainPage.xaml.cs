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

        public async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            devicePicker.Show(new Rect(0, 0, 200, 500), Windows.UI.Popups.Placement.Below);
        }

        public MainPageViewModel ViewModel
        {
            get { return (MainPageViewModel)this.DataContext; }
        }

        IGrillLibrary.IGrill igrill;
        private DevicePicker devicePicker = null;

        public MainPage()
        {
            var ncryption_key = new sbyte[] { -33, 51, -32, -119, -12, 72, 78, 115, -110, -44, -49, -71, 70, -25, -123, -74 };

            foreach (var byt in ncryption_key)
            {
                Debug.Write(String.Format("0x{0:X}, ", byt));
            }

            this.DataContext = new MainPageViewModel();

            devicePicker = new DevicePicker();
            this.devicePicker.DeviceSelected += async (devicePicker, args) =>
            {
                var device = args.SelectedDevice;
                devicePicker.Hide();

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await PairDeviceIfNecessary(device);
                    await ConnectIGrill(device.Id);
                    Settings.SelectedDeviceId = device.Id;
                });

            };
            devicePicker.Filter.SupportedDeviceSelectors.Add(BluetoothLEDevice.GetDeviceSelectorFromPairingState(false));
            devicePicker.Filter.SupportedDeviceSelectors.Add(BluetoothLEDevice.GetDeviceSelectorFromPairingState(true));

            if (Settings.SelectedDeviceId != null)
            {
                Task.Run( async () => {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await ConnectIGrill(Settings.SelectedDeviceId);
                    });
                });
            }
            this.InitializeComponent();

        }

        private async Task ConnectIGrill(string deviceId)
        {
            
            igrill = IGrillLibrary.IGrill.FromDeviceId(deviceId); 

            for (int i = 0; i < igrill.ProbeCount; i++)
            {
                ViewModel.Probes.Add(new ProbeViewModel(i));
            }

            igrill.OnTemperatureChanged += async (object sender, TemperatureChangedEventArg args) =>
            {
                Debug.WriteLine(String.Format("{0}: Probe {1} = {2}°C", DateTime.Now, args.ProbeIndex, args.Temperature));

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                     ViewModel.Probes[args.ProbeIndex].Value = args.Temperature;
                });
               
            };
            await igrill.ConnectAsync(deviceId);


            return;
            // Create a new MQTT client.
            var factory = new MqttFactory();
            var client = factory.CreateMqttClient();
            // Create TCP based options using the builder.
            var options = new MqttClientOptionsBuilder()
                .WithClientId("iGrill")
                .WithTcpServer("192.168.0.63", 1883)
                .WithCleanSession()
                .Build();
            client.Disconnected += async (s, e) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    var result = await client.ConnectAsync(options);
                }
                catch (MQTTnet.Exceptions.MqttCommunicationException ex)
                {
                    Console.WriteLine("Mqtt reconnect failed: " + ex.Message);
                }
            };


            igrill.OnTemperatureChanged += async (object sender, TemperatureChangedEventArg args) =>
            {

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("/igrill/probe" + args.ProbeIndex)
                    .WithPayload(args.Temperature.ToString())
                    .Build();

                try
                {
                    await client.PublishAsync(message);
                } catch
                {

                }
            };

            try
            {
                var result = await client.ConnectAsync(options);
            }
            catch (MQTTnet.Exceptions.MqttCommunicationException ex)
            {
                Console.WriteLine("### CONNECTING FAILED ###" + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("### CONNECTING FAILED ###" + ex.Message);
            }


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
                        throw new Exception(String.Format("Could not pair devive. Pairing status: {0}", result.Status));
                    }
                }
                throw new Exception("Pairing ot supported by device.");

            }
        }

        private void CustomPairing_PairingRequested(DeviceInformationCustomPairing sender, DevicePairingRequestedEventArgs args)
        {
            args.Accept("123456");
        }

    }
}
