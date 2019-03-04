using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGrill.Playground
{
    class MqttService
    {

        private IMqttClient client = null;



        public async Task StartAsync()
        {

            // Create a new MQTT client.
            var factory = new MqttFactory();
            client = factory.CreateMqttClient();
            // Create TCP based options using the builder.
            var options = new MqttClientOptionsBuilder()
                .WithClientId("iGrill")
                .WithTcpServer(Settings.MqttHost, Settings.MqttPort)
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

        public async Task SendProbeTemperatureAsync(int probeIndex, int? temperature)
        {
            var message = new MqttApplicationMessageBuilder()
               .WithTopic("/igrill/probe" + probeIndex)
               .WithPayload(temperature == null ? "" : temperature.ToString())
               .WithRetainFlag(true)
               .Build();
            try
            {
                await client.PublishAsync(message);
            }
            catch
            {

            }
        }

        public async Task SendBatteryLevelAsync(int? batteryLevel)
        {
            var message = new MqttApplicationMessageBuilder()
              .WithTopic("/igrill/batteryLevel")
              .WithPayload(batteryLevel == null ? "" : batteryLevel.ToString())
              .WithRetainFlag(true)
              .Build();
            try
            {
                await client.PublishAsync(message);
            }
            catch
            {

            }
        }
    }
}
