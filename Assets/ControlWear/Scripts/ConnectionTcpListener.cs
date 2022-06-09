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
        private bool _isListening;
        private TcpListener _listener = null;
        private readonly string _port;

        public ConnectionTcpListener(string port)
        {
            _port = port;
            _isListening = false;
        }

        public void Listen()
        {
            if (_isListening)
                return;

            try
            {
                _listener = new TcpListener(IPAddress.Any, int.Parse(_port));
                _listener.Start();
                _isListening = true;
                Debug.Log("Starting listening for TCP connection");
            }
            catch (SocketException e)
            {
                Debug.LogError(e.Message);
                return;
            }
            try
            {
                _listenerThread = new Thread(ListenForIncomingRequests)
                {
                    IsBackground = true
                };
                _listenerThread.Start();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

        }

        private void ListenForIncomingRequests()
        {
            try
            {
                var msg = new byte[2048];
                while (true)
                {
                    Debug.Log("Waiting for TCP connection");
                    using var client = _listener.AcceptTcpClient();
                    if (!_isListening)
                        return;
                    var stream = client.GetStream();
                    string data = null;
                    int i;
                    while ((i = stream.Read(msg, 0, msg.Length)) != 0)
                        data += Encoding.ASCII.GetString(msg, 0, i);
                    Debug.Log("Data received: " + data);
                }
            }
            catch (SocketException e) when (e.SocketErrorCode == SocketError.Interrupted)
            {
                Debug.Log("Listening canceled");
                _isListening = false;
            }
            // catch (SocketException e)
            // {
            //     Debug.LogError(e.Message);
            // }
        }

        public void Cancel()
        {
            if (!_isListening)
                return;
            
            Debug.Log("TCP listening canceled");
            _isListening = false;
            _listener.Stop();
        }
    }
}