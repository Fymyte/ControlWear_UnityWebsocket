using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using static UnityEngine.WSA.Application;

namespace Ohrizon.ControlWear.Network
{
    public class TcpListener : IListener
    {
        public event Action<string> MessageReceived;
        public event Action<string> ClientConnected;
        public event Action<string> ClientDisconnected;

        private Thread _listenerThread;
        private bool _isListening;
        private System.Net.Sockets.TcpListener _listener = null;
        private readonly string _port;

        public TcpListener(string port)
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
                _listener = new System.Net.Sockets.TcpListener(IPAddress.Any, int.Parse(_port));
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
                    using var client = _listener.AcceptTcpClient();
                    if (!_isListening)
                        return;
                    
                    InvokeOnAppThread(() => ClientConnected?.Invoke(client.ToString()), false);
                    
                    var stream = client.GetStream();
                    string data = null;
                    int i;
                    while ((i = stream.Read(msg, 0, msg.Length)) != 0)
                        data += Encoding.ASCII.GetString(msg, 0, i);
                    
                    InvokeOnAppThread(() => ClientDisconnected?.Invoke(client.ToString()), false);
                    
                    InvokeOnAppThread(() => MessageReceived?.Invoke(data), false);
                }
            }
            catch (SocketException e) when (e.SocketErrorCode == SocketError.Interrupted)
            {
                Debug.Log("Listening canceled");
                _isListening = false;
            }
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