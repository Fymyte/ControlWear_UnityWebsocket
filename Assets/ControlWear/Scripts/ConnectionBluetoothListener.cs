using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using com.ohrizon.ControlWear;
using UnityEngine;
#if ENABLE_WINMD_SUPPORT
using System.Net.Sockets;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

namespace ControlWear
{
    public class ConnectionBluetoothListener : IListener
    {
#if ENABLE_WINMD_SUPPORT
        private static readonly Guid RfcommListenerServiceUuid = Guid.Parse("5ab3b0a3-338b-4b1b-8301-fed74b25d214");
        private const byte SdpServiceNameAttributeType = (4 << 3) | 5;
        private const string SdpServiceName = "ControlWear Connection Bluetooth Listener";
        private const ushort SdpServiceNameAttributeId = 0x100;

        private Thread _listenerThread;
        private bool _isListening;
        private StreamSocket _socket;
        private DataWriter _writer;
        private DataReader _reader;
        private RfcommServiceProvider _rfcommProvider;
        private StreamSocketListener _socketListener;
#endif

        public ConnectionBluetoothListener()
        {
#if ENABLE_WINMD_SUPPORT
            this._isListening = false;
#endif
        }

        public void Listen()
        {
#if ENABLE_WINMD_SUPPORT
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
                Debug.Log(e.ToString());
            }
#else
            throw new Exception("Bluetooth connectivity isn't supported on this platform");
#endif
        }

        public void Cancel()
        {
#if ENABLE_WINMD_SUPPORT
            if (_isListening)
                Disconnect();
#else
            throw new Exception("Bluetooth connectivity isn't supported on this platform");
#endif
        }

#if ENABLE_WINMD_SUPPORT
        private async void ListenForIncomingRequests()
        {
            Debug.Log("listening for bluetooth connection...");
            try
            {
                _rfcommProvider = await RfcommServiceProvider.CreateAsync(RfcommServiceId.FromUuid(RfcommListenerServiceUuid));
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x800710DF)
            {
                Debug.Log("Make sure your Bluetooth Radio is on: " + ex.Message);
                return;
            }

            // Create a listener for this service and start listening
            _socketListener = new StreamSocketListener();
            _socketListener.ConnectionReceived += OnConnectionReceived;
            var rfcomm = _rfcommProvider.ServiceId.AsString(); 

            await _socketListener.BindServiceNameAsync(_rfcommProvider.ServiceId.AsString(),
                SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

            // Set the SDP attributes and start Bluetooth advertising
            InitializeServiceSdpAttributes(_rfcommProvider);

            try
            {
                _rfcommProvider.StartAdvertising(_socketListener, true);
            }
            catch (Exception e)
            {
                // If you aren't able to get a reference to an RfcommServiceProvider, tell the user why.  Usually throws an exception if user changed their privacy settings to prevent Sync w/ Devices.  
                Debug.Log(e.Message);
                return;
            }
            
            _isListening = true;
        }
        
        private static void InitializeServiceSdpAttributes(RfcommServiceProvider rfcommProvider)
        {
            var sdpWriter = new DataWriter();

            // Write the Service Name Attribute.
            sdpWriter.WriteByte(SdpServiceNameAttributeType);

            // The length of the UTF-8 encoded Service Name SDP Attribute.
            sdpWriter.WriteByte((byte)SdpServiceName.Length);

            // The UTF-8 encoded Service Name value.
            sdpWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            sdpWriter.WriteString(SdpServiceName);

            // Set the SDP Attribute on the RFCOMM Service Provider.
            rfcommProvider.SdpRawAttributes.Add(SdpServiceNameAttributeId, sdpWriter.DetachBuffer());
        }

        private async void OnConnectionReceived(
            StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Debug.Log("Connection received!");
            _rfcommProvider.StopAdvertising();
            _rfcommProvider = null;
            _socketListener.Dispose();
            _socketListener = null;

            _socket = args.Socket;

            var remoteDevice = await BluetoothDevice.FromHostNameAsync(_socket.Information.RemoteHostName);

            _writer = new DataWriter(_socket.OutputStream);
            _writer.UnicodeEncoding = UnicodeEncoding.Utf8;
            _reader = new DataReader(_socket.InputStream);
            _reader.UnicodeEncoding = UnicodeEncoding.Utf8;
            bool remoteDisconnection = false;

            Debug.Log("Connected to Client: " + remoteDevice.Name);

            while (true)
            {
                try
                {
                    Debug.Log("Waiting for message.");
                    // string data = null;
                    // uint i;
                    // while ((i = await reader.LoadAsync(2048)) != 0)
                    //     data += reader.ReadString(i);
                    uint readLength = await _reader.LoadAsync(sizeof(uint));
                    Debug.Log("Loaded " + readLength + "bytes");
                    // Check if the size of the data is expected (otherwise the remote has already terminated the connection)
                    if (readLength < sizeof(uint))
                    {
                        remoteDisconnection = true;
                        Debug.LogError("Failed to read message length");
                        break;
                    }
                    uint currentLength = _reader.ReadUInt32();
                    Debug.Log("message length is " + currentLength + " characters long");
                    Debug.Log("Loading message content");
                    
                    // Load the rest of the message since you already know the length of the data expected.  
                    readLength = await _reader.LoadAsync(currentLength);
                    
                    // Check if the size of the data is expected (otherwise the remote has already terminated the connection)
                    if (readLength < currentLength)
                    {
                        remoteDisconnection = true;
                        Debug.LogError("Failed to read message");
                        break;
                    }
                    string message = _reader.ReadString(currentLength);
                    
                    Debug.Log("Received: " + message);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            
            Disconnect();
        }

        private void Disconnect()
        {
            if (_rfcommProvider != null)
            {
                _rfcommProvider.StopAdvertising();
                _rfcommProvider = null;
            }

            if (_socketListener != null)
            {
                _socketListener.Dispose();
                _socketListener = null;
            }

            if (_writer != null)
            {
                _writer.DetachStream();
                _writer = null;
            }

            if (_reader != null)
            {
                _reader.DetachStream();
                _reader = null;
            }

            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }

            Debug.Log("Disconnected!");
            _isListening = false;
        }


#endif
    }
}
