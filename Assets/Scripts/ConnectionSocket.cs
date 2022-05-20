using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace ControlWear
{
    public class ConnectionSocket
    {
        private Thread _listenerThread;

        private string _port;

        public void StartSever(string port)
        {
            this._port = port;
            try
            {
                _listenerThread = new Thread(new ThreadStart(ListenForIncomingRequests));
                _listenerThread.IsBackground = true;
                _listenerThread.Start();
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        private void ListenForIncomingRequests()
        {
            try
            {
                Socket listenSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);
                // IPAddress hostIP = (Dns.GetHostEntry(IPAddress.Any.ToString())).AddressList[0];
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, int.Parse(_port));
                listenSocket.Bind(ep);
                listenSocket.Listen(5);
                var bytes = new byte[2048];
                string data = null;

                while (true)
                {
                    Debug.Log("Waiting for connection on " + "127.0.0.1" + ":" + _port + "...");
                    using (Socket client = listenSocket.Accept())
                    {
                        Debug.Log("Connected!");
                        int nbBytes = 0;
                        do
                        {
                            nbBytes = client.Receive(bytes, bytes.Length, 0);
                            data += Encoding.ASCII.GetString(bytes, 0, nbBytes);
                        } while (nbBytes > 0);
                    }

                    Debug.Log("Data received: " + data);

                    // var client = server.AcceptTcpClient();
                    // Debug.Log("Client connected");
                    // data = null;
                    //
                    // var stream = client.GetStream();
                    //
                    // int i;
                    // while ((i = stream.Read(msg, 0, msg.Length)) != 0)
                    // {
                    //     data += Encoding.ASCII.GetString(msg, 0, i);
                    // }
                    // Debug.Log("Data received: " + data);
                    //
                    // client.Close();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }
}