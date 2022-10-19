using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WPF_IoT_Chart
{

    class TemperatureMessage
    {
        public double temp { get; set; }
    }

    public class TemperatureEventArgs : EventArgs
    {
        public TemperatureEventArgs(double t)
        {
            Temperature = t;
        }
        public double Temperature { get; }
    }

    public class PropertyEventArgs: EventArgs
    {
        public PropertyEventArgs()
        {
            Props = new Dictionary<string, object>();
        }
        public Dictionary<string, object> Props{ get; set; }
    }


    class HubBrokerClient
    {
        public event EventHandler<TemperatureEventArgs>? OnTelemetryReceived;
        public event EventHandler? OnDisconnect;
        public event EventHandler<PropertyEventArgs>? OnPropertyReceived;
        public async Task ConnectAndSub()
        {


            var cs = new ConnectionSettings
            {
                HostName = "ridomqtt.centraluseuap-1.ts.eventgrid.azure.net",
                X509Key = "7E555E6FFE3D0A7F0F63A7E411094341B9293864",
            };
            var mqttClient = await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs, false);
            mqttClient.DisconnectedAsync += async ea => OnDisconnect?.Invoke(this, ea);
            

            mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                Console.WriteLine("Message received");
                Console.WriteLine($"{e.ClientId} {e.ApplicationMessage.Topic} {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                if (topic.Contains("telemetry"))
                {
                    TemperatureMessage jsonPayload = JsonSerializer.Deserialize<TemperatureMessage>(payload)!;
                    OnTelemetryReceived?.Invoke(this, new TemperatureEventArgs(jsonPayload.temp));
                    await Task.Yield();
                }
                if (topic.Contains("props/sdkInfo"))
                {
                    var propVal = JsonSerializer.Deserialize<string>(payload)!;
                    PropertyEventArgs peg = new PropertyEventArgs();
                    peg.Props.Add("sdkInfo", propVal);
                    OnPropertyReceived?.Invoke(this, peg);
                }


            };
            await mqttClient.SubscribeAsync(new MqttClientSubscribeOptionsBuilder().WithTopicFilter("device/#").Build());
            System.Diagnostics.Debug.WriteLine(mqttClient.IsConnected);
        }
    }

    internal class DisconnectedEventArgs : EventArgs
    {
        private MqttClientDisconnectReason reason;

        public DisconnectedEventArgs(MqttClientDisconnectReason reason)
        {
            this.reason = reason;
        }
    }
}
