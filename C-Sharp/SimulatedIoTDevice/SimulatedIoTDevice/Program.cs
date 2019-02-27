using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace SimulatedIoTDevice
{
    class Program
    {
        private const string DeviceConnectionString = "{insert connection string here}";
        private static int MESSAGE_COUNT = 5;
        private const int TEMPERATURE_THRESHOLD = 30;
        private static String deviceId = "{insert deviceId here}";
        private static float temperature;
        private static float humidity;
        private static Random rnd = new Random();

        static void Main(string[] args)
        {
            try
            {
                DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString, TransportType.Mqtt);

                deviceClient.OpenAsync().Wait();
                SendEvent(deviceClient).Wait();
                ReceiveCommands(deviceClient).Wait();

                Console.WriteLine("Exited!");
            }
            catch (AggregateException ex)
            {
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error in sample: {0}", exception);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error in sample: {0}", ex.Message);
            }
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }

        static async Task SendEvent(DeviceClient deviceClient)
        {
            string dataBuffer;

            Console.WriteLine("Device sending {0} messages to IoTHub...\n", MESSAGE_COUNT);

            //for (int count = 0; count < MESSAGE_COUNT; count++)
            while (true)
            {
                temperature = rnd.Next(20, 35);
                humidity = rnd.Next(60, 80);
                dataBuffer = string.Format("{{\"deviceId\":\"{0}\",\"messageId\":,\"temperature\":{1},\"humidity\":{2}}}", deviceId, temperature, humidity);
                Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
                eventMessage.Properties.Add("temperatureAlert", (temperature > TEMPERATURE_THRESHOLD) ? "true" : "false");
                Console.WriteLine("\t{0}> Sending message - Data: [{1}]", DateTime.Now.ToLocalTime(), dataBuffer);

                await deviceClient.SendEventAsync(eventMessage);
                Thread.Sleep(1000); // 1 second

            }
        }

        static async Task ReceiveCommands(DeviceClient deviceClient)
        {
            Console.WriteLine("\nDevice waiting for commands from IoTHub...\n");
            Message receivedMessage = null;

            while (true)
            {
                try
                {
                    receivedMessage = await deviceClient.ReceiveAsync(TimeSpan.FromSeconds(1));

                    if (receivedMessage != null)
                    {
                        string messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                        Console.WriteLine("\t{0}> Received message: {1}", DateTime.Now.ToLocalTime(), messageData);

                        int propCount = 0;
                        foreach (var prop in receivedMessage.Properties)
                        {
                            Console.WriteLine("\t\tProperty[{0}> Key={1} : Value={2}", propCount++, prop.Key, prop.Value);
                        }

                        await deviceClient.CompleteAsync(receivedMessage);
                    }
                }
                finally
                {
                    if (receivedMessage != null)
                    {
                        receivedMessage.Dispose();
                    }
                }
            }
        }
    }
}
