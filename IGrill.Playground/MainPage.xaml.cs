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
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace IGrill.Playground
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {


        // Using Configuration to persist selected device
        // Using Bluetooth LE
        // Using MQTT
        // Using UWP
        // Using Xaml WPF


        IGrillLibrary.IGrill igrill;
        private DevicePicker devicePicker = null;

        public MainPage()
        {
            this.InitializeComponent();

            devicePicker = new DevicePicker();
            this.devicePicker.DeviceSelected += async (devicePicker, args) =>
            {
                var device = args.SelectedDevice;
                devicePicker.Hide();

                await PairDeviceIfNecessary(device);
                await ConnectIGrill(device.Id);

            }; 
            devicePicker.Filter.SupportedDeviceSelectors.Add(BluetoothLEDevice.GetDeviceSelectorFromPairingState(false));
            devicePicker.Filter.SupportedDeviceSelectors.Add(BluetoothLEDevice.GetDeviceSelectorFromPairingState(true));
            devicePicker.Show(new Rect(0, 0, 200, 500), Windows.UI.Popups.Placement.Below);
        }

        private async Task ConnectIGrill(string deviceId)
        {
            igrill = new IGrillLibrary.IGrill();

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
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    var result = await client.ConnectAsync(options);
                }
                catch (MQTTnet.Exceptions.MqttCommunicationException ex)
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            };


            igrill.OnProbe1TemperatureChange += async (object sender, int temperature) =>
            {
                Debug.WriteLine(DateTime.Now + " Probe 1: " + temperature + "°C");
                await UpdateProbeText(probe1, temperature);
                //var message = new MqttApplicationMessageBuilder()
                //    .WithTopic("/igrill/probe1")
                //    .WithPayload(temperature.ToString())
                //    .Build();

                //await client.PublishAsync(message);

            };
            igrill.OnProbe2TemperatureChange += async (object sender, int temperature) =>
            {
                Debug.WriteLine(DateTime.Now + " Probe 2: " + temperature + "°C");
                await UpdateProbeText(probe2, temperature);
                //var message = new MqttApplicationMessageBuilder()
                //    .WithTopic("/igrill/probe2")
                //    .WithPayload(temperature.ToString())
                //    .Build();

                //await client.PublishAsync(message);
            };
            igrill.OnProbe3TemperatureChange += async (object sender, int temperature) =>
            {
                Debug.WriteLine(DateTime.Now + " Probe 3: " + temperature + "°C");
                await UpdateProbeText(probe3, temperature);
                //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                //    probe3.Text = temperature + "°C";
                //});
                //var message = new MqttApplicationMessageBuilder()
                //    .WithTopic("/igrill/probe3")
                //    .WithPayload(temperature.ToString())
                //    .Build();

                //await client.PublishAsync(message);

            };
            igrill.OnProbe4TemperatureChange += async (object sender, int temperature) =>
            {
                Debug.WriteLine(DateTime.Now + " Probe 4: " + temperature + "°C");
                await UpdateProbeText(probe4, temperature);
                //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                //    probe4.Text = temperature + "°C";
                //});
                //var message = new MqttApplicationMessageBuilder()
                //    .WithTopic("/igrill/probe4")
                //    .WithPayload(temperature.ToString())
                //    .Build();

                //await client.PublishAsync(message);
            };

            try
            {
                await igrill.InitAsync(deviceId);
//                var result = await client.ConnectAsync(options);
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

        private async Task UpdateProbeText(TextBlock textBox, int temperature)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                textBox.Text = temperature + "°C";
            });
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
