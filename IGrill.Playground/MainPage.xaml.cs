using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        IGrillLibrary.IGrill igrill = new IGrillLibrary.IGrill();


        public MainPage()
        {
            this.InitializeComponent();

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
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    probe1.Text = temperature + "°C";
                });
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("/igrill/probe1")
                    .WithPayload(temperature.ToString())
                    .Build();

                await client.PublishAsync(message);

            };
            igrill.OnProbe2TemperatureChange += async (object sender, int temperature) =>
            {
                Debug.WriteLine(DateTime.Now + " Probe 2: " + temperature + "°C");
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    probe2.Text = temperature + "°C";
                });
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("/igrill/probe2")
                    .WithPayload(temperature.ToString())
                    .Build();

                await client.PublishAsync(message);
            };
            igrill.OnProbe3TemperatureChange += async (object sender, int temperature) =>
            {
                Debug.WriteLine(DateTime.Now + " Probe 3: " + temperature + "°C");
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    probe3.Text = temperature + "°C";
                });
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("/igrill/probe3")
                    .WithPayload(temperature.ToString())
                    .Build();

                await client.PublishAsync(message);

            };
            igrill.OnProbe4TemperatureChange += async (object sender, int temperature) =>
            {
                Debug.WriteLine(DateTime.Now + " Probe 4: " + temperature + "°C");
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    probe4.Text = temperature + "°C";
                });
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("/igrill/probe4")
                    .WithPayload(temperature.ToString())
                    .Build();

                await client.PublishAsync(message);
            };
            Task.Run(async () =>
            {
                try
                {
                    await igrill.InitAsync();
                    var result = await client.ConnectAsync(options);
                }
                catch (MQTTnet.Exceptions.MqttCommunicationException ex)
                {
                    Console.WriteLine("### CONNECTING FAILED ###");
                } 
                catch (Exception ex)
                {

                }
            });


        }
    }
}
