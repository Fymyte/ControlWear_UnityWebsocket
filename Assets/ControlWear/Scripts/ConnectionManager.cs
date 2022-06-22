using System;
using System.Collections.Generic;
using System.Linq;
using Ohrizon.ControlWear.Gesture;
using Ohrizon.ControlWear.DollarQ;
using Ohrizon.ControlWear.Network;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    private TcpListener _tcpListener;
    private GestureManager _gestureManager;

    private BluetoothListener _bluetoothListener;
    private void Start()
    {
        _gestureManager = new GestureManager();
        // _tcpListener = new TcpListener("54321");
        _bluetoothListener = new BluetoothListener();
        
        // _tcpListener.MessageReceived += OnMessageReceived;
        // _tcpListener.ClientDisconnected += OnClientDisconnected;
        // _tcpListener.ClientConnected += OnClientConnected;
        _bluetoothListener.MessageReceived += OnMessageReceived;
        _bluetoothListener.ClientDisconnected += OnClientDisconnected;
        _bluetoothListener.ClientConnected += OnClientConnected;

        _gestureManager.SingleTap += () => Debug.Log("Single Tap");
        // _gestureManager.PointerRelease += OnPointerRelease;
        _gestureManager.DoubleTap += () => Debug.Log("Double Tap");
        _gestureManager.LongTap += () => Debug.Log("Long Tap");
        // _gestureManager.PointerDown += (x, y) => Debug.Log($"Pointer Down ({x}, {y})");
        // _gestureManager.PointerUp += (x, y) => Debug.Log($"Pointer Up ({x}, {y})");
        // _gestureManager.PointerMove += (x, y) => Debug.Log($"Pointer Move ({x}, {y})");
        // _gestureManager.PointerUpdate += (x, y) => Debug.Log($"Pointer Update ({x}, {y})");
        _gestureManager.Scroll += delta => Debug.Log($"Scroll ({delta})");
        void OnBack() => Debug.Log("OnBack");
        void OnForward() => Debug.Log("OnForward");
        void OnUp() => Debug.Log("OnUp");
        void OnDown() => Debug.Log("OnDown");
        _gestureManager.ArrowDown += OnDown;
        _gestureManager.VerticalUp += OnDown;
        _gestureManager.ArrowUp += OnUp;
        _gestureManager.VerticalDown += OnUp;
        _gestureManager.ArrowLeft += OnBack;
        _gestureManager.HorizontalRight += OnBack;
        _gestureManager.ArrowRight += OnForward;
        _gestureManager.HorizontalLeft += OnForward;
        
        // _tcpListener.Listen();
        _bluetoothListener.Listen();
    }

    private void OnPointerRelease(string gesture, float distance)
    {
        Debug.Log($"Recognized class: {gesture}");
    }

    private void OnClientDisconnected(string device)
    {
        Debug.Log($"Client {device} disconnected");
        // _tcpListener.Listen();
        _bluetoothListener.Listen();
    }

    private void OnClientConnected(string device)
    {
        Debug.Log($"Client {device} connected");
    }

    private void OnMessageReceived(string message)
    {
        _gestureManager.Recognize(message); 
        // Debug.Log("Message received: " + message);
    }
}
