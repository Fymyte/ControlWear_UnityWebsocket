using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using com.ohrizon.ControlWear;
using UnityEngine;
#if ENABLE_WINMD_SUPPORT
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
        private static readonly byte SdpServiceNameAttributeType = (4 << 3) | 5;
        private static readonly string SdpServiceName = "ControlWear Connection Bluetooth Listener";
        private static readonly UInt16 SdpServiceNameAttributeId = 0x100;

        private Thread _listenerThread;
        private StreamSocket socket;
        private DataWriter writer;
        private DataReader reader;
        private RfcommServiceProvider rfcommProvider;
        private StreamSocketListener socketListener;
#endif

        public ConnectionBluetoothListener()
        {

        }

        public void Listen()
        {
#if ENABLE_WINMD_SUPPORT
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
                rfcommProvider = await RfcommServiceProvider.CreateAsync(RfcommServiceId.FromUuid(RfcommListenerServiceUuid));
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x800710DF)
            {
                Debug.Log("Make sure your Bluetooth Radio is on: " + ex.Message);
                return;
            }

            // Create a listener for this service and start listening
            socketListener = new StreamSocketListener();
            socketListener.ConnectionReceived += OnConnectionReceived;
            var rfcomm = rfcommProvider.ServiceId.AsString(); 

            await socketListener.BindServiceNameAsync(rfcommProvider.ServiceId.AsString(),
                SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

            // Set the SDP attributes and start Bluetooth advertising
            InitializeServiceSdpAttributes(rfcommProvider);

            try
            {
                rfcommProvider.StartAdvertising(socketListener, true);
            }
            catch (Exception e)
            {
                // If you aren't able to get a reference to an RfcommServiceProvider, tell the user why.  Usually throws an exception if user changed their privacy settings to prevent Sync w/ Devices.  
                Debug.Log(e.Message);
                return;
            }

        }
        
        private void InitializeServiceSdpAttributes(RfcommServiceProvider rfcommProvider)
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
            socketListener.Dispose();
            socketListener = null;

            try
            {
                socket = args.Socket;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to retrieve args: " + e.Message);
                Disconnect();
                return;
            }

            var remoteDevice = await BluetoothDevice.FromHostNameAsync(socket.Information.RemoteHostName);

            writer = new DataWriter(socket.OutputStream);
            writer.UnicodeEncoding = UnicodeEncoding.Utf8;
            reader = new DataReader(socket.InputStream);
            reader.UnicodeEncoding = UnicodeEncoding.Utf8;
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
                    uint readLength = await reader.LoadAsync(sizeof(uint));
                    Debug.Log("Loaded " + readLength + "bytes");
                    // Check if the size of the data is expected (otherwise the remote has already terminated the connection)
                    if (readLength < sizeof(uint))
                    {
                        remoteDisconnection = true;
                        Debug.LogError("Failed to read message length");
                        break;
                    }
                    uint currentLength = reader.ReadUInt32();
                    Debug.Log("message length is " + currentLength + " characters long");
                    Debug.Log("Loading message content");
                    
                    // Load the rest of the message since you already know the length of the data expected.  
                    readLength = await reader.LoadAsync(currentLength);
                    
                    // Check if the size of the data is expected (otherwise the remote has already terminated the connection)
                    if (readLength < currentLength)
                    {
                        remoteDisconnection = true;
                        Debug.LogError("Failed to read message");
                        break;
                    }
                    string message = reader.ReadString(currentLength);
                    
                    Debug.Log("Received: " + message);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        private void Disconnect()
        {
            if (rfcommProvider != null)
            {
                rfcommProvider.StopAdvertising();
                rfcommProvider = null;
            }

            if (socketListener != null)
            {
                socketListener.Dispose();
                socketListener = null;
            }

            if (writer != null)
            {
                writer.DetachStream();
                writer = null;
            }

            if (socket != null)
            {
                socket.Dispose();
                socket = null;
            }

            Debug.Log("Diconnected!");
        }


#endif
    }
}
