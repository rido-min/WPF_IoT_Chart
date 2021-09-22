using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using MQTTnet.Diagnostics;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPF_IoT_Chart
{

    class TemperatureMessage
    {
        public double Temperature { get; set; }
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
        public event EventHandler? OnDisconnect;
        public async Task ConnectAndSub()
        {

            var hostname = "ridobrk2.azure-devices.net";
            var deviceId = "op1";
            var certPath = $"../../../../certs/{deviceId}.pfx";
            using var cert = new X509Certificate2(certPath, "1234");
            var cid = cert.Subject.Substring(3);
            List<X509Certificate> certs = new List<X509Certificate> { cert };

            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();

            var username = $"av=2021-06-30-preview&h={hostname}&did={cid}&am=X509";

            var options = new MqttClientOptionsBuilder()
               .WithClientId(cid)
               .WithTcpServer(hostname, 8883)
               .WithTls(new MqttClientOptionsBuilderTlsParameters
               {
                   UseTls = true,
                   SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
                   Certificates = certs
               })
               .WithCredentials(new MqttClientCredentials()
               {
                   Username = username,
                   Password = Encoding.UTF8.GetBytes("")
               })
               .Build();

            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                Console.WriteLine("Message received");
                Console.WriteLine($"{e.ClientId} {e.ApplicationMessage.Topic} {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                TemperatureMessage jsonPayload = System.Text.Json.JsonSerializer.Deserialize<TemperatureMessage>(payload);

                OnTelemetryReceived?.Invoke(this, new TemperatureEventArgs(jsonPayload.Temperature));
            });
            mqttClient.UseConnectedHandler(async e =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");
                await mqttClient.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
                                                        .WithTopicFilter("sample/topic/telemetry", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                                        .Build());

            });

            mqttClient.UseDisconnectedHandler(e => 
            {
                System.Diagnostics.Debug.WriteLine(e.Reason);
                OnDisconnect?.Invoke(this, new DisconnectedEventArgs(e.Reason));
            });

            //MqttNetGlobalLogger.LogMessagePublished += (s, e) =>
            //{
            //    var trace = $">> [{e.TraceMessage.Timestamp:O}] [{e.TraceMessage.ThreadId}] [{e.TraceMessage.Source}] [{e.TraceMessage.Level}]: {e.TraceMessage.Message}";
            //    if (e.TraceMessage.Exception != null)
            //    {
            //        trace += Environment.NewLine + e.TraceMessage.Exception.ToString();
            //    }

            //    System.Diagnostics.Debug.WriteLine(trace);
            //};

            await mqttClient.ConnectAsync(options, CancellationToken.None);
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
