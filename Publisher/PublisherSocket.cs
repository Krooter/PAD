
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Publisher
{
    public class PublisherSocket
    {
        private Socket _socket;
        public bool IsConnected;
        public PublisherSocket()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(string ipAddress, int port)
        {
            _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ipAddress), port), ConnectedCallBack, null); //varianta asynchrona pentru connect.
            Thread.Sleep(2000);
        }

        private void ConnectedCallBack(IAsyncResult asyncResult)
        {
            if (_socket.Connected)
            {
                Console.WriteLine("Sender connected to Broker.");
            }
            else
            {
                Console.WriteLine("Error: Sender not connected to Broker.");
            }

            IsConnected = _socket.Connected;
        }

        internal void Send(byte[] data)
        {
            try
            {
                _socket.Send(data);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Could not send data!", ex.Message);
            }
        }
    }
}
