using Common;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            var publisherSocket = new PublisherSocket();

            publisherSocket.Connect(Settings.BROKER_IP, Settings.BROKER_PORT);

            if (publisherSocket.IsConnected)
            {
                while (true)
                {
                    var payload = new Payload();
                    Console.WriteLine("Enter topic:");
                    payload.Topic = Console.ReadLine().ToLower();

                    Console.WriteLine("Enter message:");
                    payload.Message = Console.ReadLine();

                    var payLoadString = JsonConvert.SerializeObject(payload);
                    byte[] data = Encoding.UTF8.GetBytes(payLoadString);

                    publisherSocket.Send(data);
                }
            }
        }
    }
}
