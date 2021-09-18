
using Common;
using System;
using System.Net;
using System.Net.Sockets;

namespace Broker
{
    public class BrokerSocket
    {
        private Socket _socket;
        private const int CONNECTION_LIMIT = 10;

        public BrokerSocket()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start(string ipAddress, int port)
        {
            _socket.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));
            _socket.Listen(CONNECTION_LIMIT);

            Accept();
        }

        private void Accept()
        {
            _socket.BeginAccept(AcceptedCallBack, null); //asynchron method
        }

        private void AcceptedCallBack(IAsyncResult ar)
        {
            ConnectionInfo connection = new ConnectionInfo();
            try
            {
                connection.Socket = _socket.EndAccept(ar);
                connection.Address = connection.Socket.RemoteEndPoint.ToString();
                connection.Socket.BeginReceive(connection.Data, 0, connection.Data.Length, 
                    SocketFlags.None, ReceiveCallBack, connection);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Can't accept. {ex.Message}");
            }
            finally
            {
                Accept();
            }
        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            ConnectionInfo connection = ar.AsyncState as ConnectionInfo;

            try
            {
                Socket senderSocket = connection.Socket;
                SocketError response;

                int buffSize = senderSocket.EndReceive(ar, out response);

                if(response == SocketError.Success)
                {
                    byte[] payload = new byte[buffSize];
                    Array.Copy(connection.Data, payload, buffSize);
                }
            } 
            catch(Exception ex)
            {
                Console.WriteLine($"Can't receive data: {ex.Message}");
            }
            finally
            {
                try
                {
                    connection.Socket.BeginReceive(connection.Data, 0, connection.Data.Length, 
                        SocketFlags.None, ReceiveCallBack, connection);
                } catch(Exception ex)
                {
                    Console.Write($"{ex.Message}");
                    var address = connection.Socket.RemoteEndPoint.ToString();

                    connection.Socket.Close();
                }
            }
        }
    }
}
