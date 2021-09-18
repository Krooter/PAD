using Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Subscriber
{
    class SubscriberSocket
    {
        private Socket _socket;
        private string _topic;

        public SubscriberSocket(string topic)
        {
            _topic = topic;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(string ipAdress, int port)
        {
            _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ipAdress), port), ConnectedCallBack, null);
            Console.WriteLine("Waiting for a connection");
        }

        private void ConnectedCallBack(IAsyncResult ar)
        {
            if (_socket.Connected)
            {
                Console.WriteLine("Subscriber connected to broker.");
                Subscribe();
                StartReceive();
            }
            else
            {
                Console.WriteLine("Error: Subscriber could not connect to broker.");
            }
        }

        private void Subscribe()
        {
            var data = Encoding.UTF8.GetBytes("subscribe#" + _topic);
            Send(data);
        }

        private void StartReceive()
        {
            ConnectionInfo connection = new ConnectionInfo();
            connection.Socket = _socket;

            _socket.BeginReceive(connection.Data, 0, connection.Data.Length, 
                SocketFlags.None, ReceiveCallBack, connection);
        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            ConnectionInfo connectionInfo = ar.AsyncState as ConnectionInfo;

            try
            {
                SocketError response;
                int buffSize = _socket.EndReceive(ar, out response);

                if(response == SocketError.Success)
                {
                    byte[] payloadBytes = new byte[buffSize];
                    Array.Copy(connectionInfo.Data, payloadBytes, payloadBytes.Length);

                    PayloadHandler.Handle(payloadBytes);
                }
            }catch(Exception ex)
            {
                Console.WriteLine($"Can't receive data from broker. {ex.Message}");
            }
            finally
            {
                try
                {
                    connectionInfo.Socket.BeginReceive(connectionInfo.Data, 0, connectionInfo.Data.Length,
                        SocketFlags.None, ReceiveCallBack, connectionInfo);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                    connectionInfo.Socket.Close();
                }
            }
        }

        private void Send(byte[] data)
        {
            try
            {
                _socket.Send(data);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Could not send data: {ex.Message}");
            }
        }
    }
}
