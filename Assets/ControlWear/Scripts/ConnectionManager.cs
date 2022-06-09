using System;
using System.Collections;
using System.Collections.Generic;
using ControlWear;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    private ConnectionTcpListener _tcpListener;

    private int counter = 0;

    private ConnectionBluetoothListener _bluetoothListener;
    // Start is called before the first frame update
    void Start()
    {
        _tcpListener = new ConnectionTcpListener("54123");
        _bluetoothListener = new ConnectionBluetoothListener();
        _tcpListener.Listen();
        _bluetoothListener.Listen();
        counter = 0;
        // tcpListener.Cancel();
    }

    private void Update()
    {
        counter += 1;
        if (counter == 360)
            _bluetoothListener.Cancel();
    }
}
