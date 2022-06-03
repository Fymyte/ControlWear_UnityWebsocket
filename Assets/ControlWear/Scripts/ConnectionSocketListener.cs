using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using com.ohrizon.ControlWear;
using UnityEngine;

namespace ControlWear
{
    public class ConnectionSocketListener : IListener
    {
        private Thread _listenerThread;

        private string _port;

        public ConnectionSocketListener(string port)
        {
            this._port = port;
        }

        public void Listen()
        {
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
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, int.Parse(_port));
                listenSocket.Bind(ep);
                listenSocket.Listen(5);
                var bytes = new byte[2048];

                while (true)
                {
                    using Socket client = listenSocket.Accept();
                    int nbBytes;
                    var data = "";
                    while ((nbBytes = client.Receive(bytes, bytes.Length, 0)) != 0)
                        data += Encoding.ASCII.GetString(bytes, 0, nbBytes);
                    Debug.Log("Data received: " + data);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }
}