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
        #region Events

        public event Action<string> MessageReceived;
        public event Action<string> ClientConnected;
        public event Action<string> ClientDisconnected;


        #endregion
        
        #region Private attributes
        
        private Thread _listenerThread;
        private bool _isListening;
        private System.Net.Sockets.TcpListener _listener;
        private readonly string _port;
        private string _clientName;
        
        #endregion

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
                Debug.Log($"Starting listening for TCP connection on port {_port}");
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

        private async void ListenForIncomingRequests()
        {
            TcpClient client;
            try
            {
                client = await _listener.AcceptTcpClientAsync();
                if (!_isListening) return;
            }
            catch (SocketException e)
            {
                Debug.Log("Failed to accept client: " + e.Message);
                return;
            }

            bool remoteDisconnection = false;
            using (client)
            {
                _clientName = client.Client.RemoteEndPoint.ToString(); 
                InvokeOnAppThread(() => ClientConnected?.Invoke(_clientName), false);
                var stream = client.GetStream();
                
                while (true)
                {
                    if (!_isListening) break;
                    try
                    {
                        var msg = new byte[sizeof(int)];
                        var readLength = stream.Read(msg, 0, sizeof(int));
                        if (readLength < sizeof(int))
                        {
                            remoteDisconnection = true;
                            break;
                        }

                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(msg);
                        var currentLength = BitConverter.ToInt32(msg, 0);
                        Debug.Log("message length: " + currentLength);
                        // A message with more than 100.000 character is probably an error
                        if (currentLength > 100000)
                        {
                            Debug.LogError("Message length too high, skipping");
                            continue;
                        }
                        msg = new byte[currentLength];
                        var message = "";
                        readLength = 0;
                        int i;
                        do
                        {
                            i = stream.Read(msg, 0, currentLength - readLength);
                            readLength += i;
                            message += Encoding.UTF8.GetString(msg, 0, i);
                        } while (i != 0 && readLength < currentLength);
                            
                        if (readLength < currentLength)
                        {
                            remoteDisconnection = true;
                            Debug.LogError("Failed to read message (read length: " + readLength + ")");
                            break;
                        }

                        InvokeOnAppThread(() => MessageReceived?.Invoke(message), false);
                    }
                    catch (SocketException e) when (e.SocketErrorCode == SocketError.Interrupted)
                    {
                        InvokeOnAppThread(() => ClientDisconnected?.Invoke(_clientName??"None"), false);
                        Debug.Log("Listening canceled");
                        _isListening = false;
                    }
                }
                
            }

            if (!remoteDisconnection) return;
            Disconnect();
        }

        private void Disconnect()
        {
            _isListening = false;
            if (_listener != null)
            {
                _listener.Stop();
                _listener = null;
            }
            InvokeOnAppThread(() => ClientDisconnected?.Invoke(_clientName), false);
        }

        public void Cancel()
        {
            if (_isListening)
                Disconnect();
        }
    }
}