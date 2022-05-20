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
    public class Connection
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
            TcpListener server = null;
            try
            {
                var listenAddress = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(listenAddress, int.Parse(_port));
                server.Start();

                var msg = new byte[2048];
                string data = null;

                while (true)
                {
                    Debug.Log("Waiting for connection on " + listenAddress + ":" + _port + "...");
                    var client = server.AcceptTcpClient();
                    Debug.Log("Client connected");
                    data = null;

                    var stream = client.GetStream();

                    int i;
                    while ((i = stream.Read(msg, 0, msg.Length)) != 0)
                    {
                        data += Encoding.ASCII.GetString(msg, 0, i);
                    }
                    Debug.Log("Data received: " + data);
                    
                    client.Close();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }
}