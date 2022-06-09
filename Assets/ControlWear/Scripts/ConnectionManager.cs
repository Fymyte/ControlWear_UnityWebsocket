using System;
using System.Collections;
using System.Collections.Generic;
using ControlWear;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    private ConnectionTcpListener _tcpListener;

    private ConnectionBluetoothListener _bluetoothListener;
    // Start is called before the first frame update
    private void Start()
    {
        _tcpListener = new ConnectionTcpListener("54123");
        _bluetoothListener = new ConnectionBluetoothListener();
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
