using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Text;
using System.Threading.Tasks;
using mqttdeviceProtos;

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

    class HubBrokerClient
    {
        public event EventHandler<TemperatureEventArgs>? OnTelemetryReceived;
        //public event EventHandler? OnDisconnect;
        public async Task ConnectAndSub()
        {


            var cs = new ConnectionSettings
            {
                HostName = "ridomqtt.centraluseuap-1.ts.eventgrid.azure.net",
                X509Key = "7E555E6FFE3D0A7F0F63A7E411094341B9293864",
            };
            var mqttClient = await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs, false);

            mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                Console.WriteLine("Message received");
                Console.WriteLine($"{e.ClientId} {e.ApplicationMessage.Topic} {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                TemperatureMessage jsonPayload = System.Text.Json.JsonSerializer.Deserialize<TemperatureMessage>(payload);

                //var tel = Telemetry.Parser.ParseFrom(e.ApplicationMessage.Payload);
                OnTelemetryReceived?.Invoke(this, new TemperatureEventArgs(jsonPayload.temp));
                await Task.Yield();
            };
            await mqttClient.SubscribeAsync(new MqttClientSubscribeOptionsBuilder().WithTopicFilter("device/+/telemetry/temp").Build());

            
            //MqttNetGlobalLogger.LogMessagePublished += (s, e) =>
            //{
            //    var trace = $">> [{e.TraceMessage.Timestamp:O}] [{e.TraceMessage.ThreadId}] [{e.TraceMessage.Source}] [{e.TraceMessage.Level}]: {e.TraceMessage.Message}";
            //    if (e.TraceMessage.Exception != null)
            //    {
            //        trace += Environment.NewLine + e.TraceMessage.Exception.ToString();
            //    }

            //    System.Diagnostics.Debug.WriteLine(trace);
            //};

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
