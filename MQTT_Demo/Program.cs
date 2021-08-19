using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Client;
using System;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string broker = "";
            int port = 0;
            string mqttUsername = "";
            string mqttPswd = "";

            string topicPub = "zigbee2mqtt/Zarovka/set";
            string tupicSub = "zigbee2mqtt/Zarovka";
            string stateOn = "{\"state\": \"ON\"}";
            string stateOff = "{\"state\": \"OFF\"}";


            var options = new MqttClientOptionsBuilder()
                            .WithClientId("IoTHUB-Demo")
                            .WithTcpServer(broker, port)
                            .WithCredentials(mqttUsername, mqttPswd)
                            .WithCleanSession()
                            .Build();

            IMqttClient client = new MqttFactory().CreateMqttClient();

            client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
            client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);

            client.UseApplicationMessageReceivedHandler(OnMessage);

            try
            {
                await client.ConnectAsync(options);
            }
            catch (MQTTnet.Exceptions.MqttCommunicationTimedOutException ex)
            {
                Console.WriteLine("Connection to MQTT broker can not be established: " + ex.Message);
                Environment.Exit(-1);
            }
            catch (MQTTnet.Adapter.MqttConnectingFailedException ex)
            {
                Console.WriteLine("Connection to MQTT broker can not be established: " + ex.Message);
                Environment.Exit(-1);
            }

            await client.SubscribeAsync(new MqttTopicFilterBuilder()
                        .WithTopic(tupicSub)
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build());

            while(true)
            {
                showMenu();
                switch (Console.ReadLine())
                {
                    case "0":
                        Console.WriteLine(topicPub + "\n" + stateOff);
                        await client.PublishAsync(topicPub, stateOff);
                        break;
                    case "1":
                        Console.WriteLine(topicPub + "\n" + stateOn);
                        await client.PublishAsync(topicPub, stateOn);
                        break;
                    case "2":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Wrong selection");
                        break;
                }
            }

        }

        public static void showMenu()
        {
            Console.WriteLine("==============");
            Console.WriteLine("0 => Turn OFF\n1 => Turn ON\n2 => Exit");
            Console.WriteLine("==============");
        }

        public static void OnConnected(MqttClientConnectedEventArgs obj)
        {
            Console.WriteLine("Succesfully connected");
        }

        public static void OnDisconnected(MqttClientDisconnectedEventArgs obj)
        {
            Console.WriteLine("Succesfully disconnected");
        }

        public static void OnMessage(MqttApplicationMessageReceivedEventArgs obj)
        {
            Console.WriteLine("\nNew message:\n" + Encoding.UTF8.GetString(obj.ApplicationMessage.Payload));
            showMenu();
        }
    }
}
