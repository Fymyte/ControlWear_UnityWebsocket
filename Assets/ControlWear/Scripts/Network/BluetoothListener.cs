using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.WSA.Application;
#if ENABLE_WINMD_SUPPORT
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

namespace Ohrizon.ControlWear.Network
{
    public class BluetoothListener : IListener
    {
        #region Events

        public event Action<string> MessageReceived;
        public event Action<string> ClientConnected;
        public event Action<string> ClientDisconnected;

        #endregion

#if ENABLE_WINMD_SUPPORT

        #region Constants

        private static readonly Guid RfcommListenerServiceUuid = Guid.Parse("5ab3b0a3-338b-4b1b-8301-fed74b25d214");
        private const byte SdpServiceNameAttributeType = (4 << 3) | 5;
        private const string SdpServiceName = "ControlWear Connection Bluetooth Listener";
        private const ushort SdpServiceNameAttributeId = 0x100;

        #endregion

        #region Private attributes

        private Thread _listenerThread;
        private bool _isBluetoothDiscoverable;
        private bool _isListening;
        private StreamSocket _socket;
        private DataWriter _writer;
        private RfcommServiceProvider _rfcommProvider;
        private StreamSocketListener _socketListener;
        private string _remoteDeviceName;

        #endregion
#endif

        public BluetoothListener()
        {
#if ENABLE_WINMD_SUPPORT
            _isBluetoothDiscoverable = false;
            _isListening = false;
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
            await RegisterForInboundPairingRequest();
            var deviceInfo = new EasClientDeviceInformation();
            Debug.Log($"Listening for bluetooth connection as {deviceInfo.FriendlyName}...");
            _isListening = true;
        }

        private async void OnConnectionReceived(
            StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Debug.Log("Connection received!");
            _socketListener.Dispose();
            _socketListener = null;
            _isBluetoothDiscoverable = false;

            _socket = args.Socket;

            var remoteDevice = await BluetoothDevice.FromHostNameAsync(_socket.Information.RemoteHostName);
            _remoteDeviceName = remoteDevice.Name;

            _writer = new DataWriter(_socket.OutputStream) { UnicodeEncoding = UnicodeEncoding.Utf8 };
            var reader = new DataReader(_socket.InputStream) { UnicodeEncoding = UnicodeEncoding.Utf8 };
            var remoteDisconnection = false;

            InvokeOnAppThread(() => { ClientConnected?.Invoke(_remoteDeviceName); }, false);

            while (true)
            {
                try
                {
                    var readLength = await reader.LoadAsync(sizeof(uint));
                    if (readLength < sizeof(uint))
                    {
                        remoteDisconnection = true;
                        break;
                    }

                    var currentLength = reader.ReadInt32();

                    // Load the rest of the message since you already know the length of the data expected.  
                    readLength = await reader.LoadAsync((uint)currentLength);

                    // Check if the size of the data is expected (otherwise the remote has already terminated the connection)
                    if (readLength < currentLength)
                    {
                        remoteDisconnection = true;
                        Debug.LogError("Failed to read message (read length: " + readLength + ")");
                        break;
                    }

                    var message = reader.ReadString((uint)currentLength);

                    InvokeOnAppThread(() => { MessageReceived?.Invoke(message); }, false);
                }
                catch (Exception ex) when ((uint)ex.HResult == 0x800703E3)
                {
                    break;
                }
            }

            reader.DetachStream();

            if (!remoteDisconnection) return;

            Disconnect();
        }

        private void Disconnect()
        {
            _isListening = false;
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

            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }

            InvokeOnAppThread(() => ClientDisconnected?.Invoke(_remoteDeviceName), false);
        }

        private async Task RegisterForInboundPairingRequest()
        {
            // Make the system discoverable for bluetooth
            await MakeDiscoverable();
            // If the attempt to make the system discoverable failed then likely there is no Bluetooth device present
            // so leave the diagnostic message put out by the call to MakeDiscoverable()

            // if (!DeviceInformationPairing.TryRegisterForAllInboundPairingRequests(ceremoniesSelected))
            //     Debug.LogError("Unable to register for selected pairing kind");

            // TODO: Bind to windows "OnActivated" function with ActivationKind "DevicePairing" to properly handle connection     
        }

        // private static DevicePairingKinds GetSelectedCeremonies()
        // {
        //     return DevicePairingKinds.ConfirmOnly
        //            // | DevicePairingKinds.DisplayPin
        //            // | DevicePairingKinds.ProvidePin
        //            | DevicePairingKinds.ConfirmPinMatch;
        // }

        private async Task MakeDiscoverable()
        {
            // Don't repeatedly do this or the StartAdvertising will throw "cannot create a file when that file already exists"
            if (_isBluetoothDiscoverable)
                return;

            try
            {
                _rfcommProvider =
                    await RfcommServiceProvider.CreateAsync(RfcommServiceId.FromUuid(RfcommListenerServiceUuid));

                // Create a listener for this service and start listening
                _socketListener = new StreamSocketListener();
                _socketListener.ConnectionReceived += OnConnectionReceived;

                await _socketListener.BindServiceNameAsync(_rfcommProvider.ServiceId.AsString(),
                    SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);
                // Set the SDP attributes and start Bluetooth advertising
                InitializeServiceSdpAttributes(_rfcommProvider);
                _rfcommProvider.StartAdvertising(_socketListener, true);
                _isBluetoothDiscoverable = true;
            }
            catch (Exception ex) when ((uint)ex.HResult == 0x800710DF)
            {
                Debug.Log("Make sure your Bluetooth Radio is on: " + ex.Message);
            }
            catch (Exception e)
            {
                // If you aren't able to get a reference to an RfcommServiceProvider, tell the user why.  Usually throws an exception if user changed their privacy settings to prevent Sync w/ Devices.  
                Debug.Log(e.Message);
            }
        }

        private static void InitializeServiceSdpAttributes(RfcommServiceProvider rfcommProvider)
        {
            var sdpWriter = new DataWriter();

            // Write the Service Name Attribute.
            sdpWriter.WriteByte(SdpServiceNameAttributeType);

            // The length of the UTF-8 encoded Service Name SDP Attribute.
            sdpWriter.WriteByte((byte)SdpServiceName.Length);

            // The UTF-8 encoded Service Name value.
            sdpWriter.UnicodeEncoding = UnicodeEncoding.Utf8;
            sdpWriter.WriteString(SdpServiceName);

            // Set the SDP Attribute on the RFCOMM Service Provider.
            rfcommProvider.SdpRawAttributes.Add(SdpServiceNameAttributeId, sdpWriter.DetachBuffer());
        }

#endif
    }
}