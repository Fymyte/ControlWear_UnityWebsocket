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
    public class ConnectionTcpListener : IListener
    {
        private Thread _listenerThread;
        private string _port;

        public ConnectionTcpListener(string port)
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
                var server = new TcpListener(IPAddress.Any, int.Parse(_port));
                server.Start();

                var msg = new byte[2048];

                while (true)
                {
                    using var client = server.AcceptTcpClient();
                    var stream = client.GetStream();
                    string data = null;
                    int i;
                    while ((i = stream.Read(msg, 0, msg.Length)) != 0)
                        data += Encoding.ASCII.GetString(msg, 0, i);
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