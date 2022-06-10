using Ohrizon.ControlWear.Network;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    private TcpListener _tcpListener;

    private BluetoothListener _bluetoothListener;
    // Start is called before the first frame update
    private void Start()
    {
        _tcpListener = new TcpListener("54123");
        _bluetoothListener = new BluetoothListener();
        _tcpListener.MessageReceived += OnMessageReceived;
        _bluetoothListener.MessageReceived += OnMessageReceived;
        _tcpListener.Listen();
        _bluetoothListener.Listen();
    }

    private static void OnMessageReceived(string message)
    {
        Debug.Log("Received on main thread'" + message + "'");
    }
}
