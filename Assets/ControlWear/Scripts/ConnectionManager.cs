using System;
using System.Collections.Generic;
using Ohrizon.ControlWear.Action;
using Ohrizon.ControlWear.DollarQ;
using Ohrizon.ControlWear.Network;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [Serializable]
    public class WrapperPoint
    {
        public Point[] points;
    }

    private static List<Gesture> _gestures = new List<Gesture>
    {
        new Gesture(new Point[] {
            new Point(30, 7, 1), new Point(103, 7, 1),
            new Point(66, 7, 2), new Point(66, 87, 2)
        }, "T"),
        new Gesture(new Point[]
        {
           new Point(30, 20, 1), new Point(200, 20, 1)
        }, "Straight line"),
        new Gesture(new Point[] {
            new Point(177, 92, 1), new Point(177, 2, 1),
            new Point(182, 1, 2), new Point(246, 95, 2),
            new Point(247, 87, 3), new Point(247, 1, 3)
        }, "N"),
        new Gesture(new Point[] {
            new Point(345, 9, 1), new Point(345, 87, 1),
            new Point(351, 8, 2), new Point(363, 8, 2), new Point(372, 9, 2), new Point(380, 11, 2), new Point(386, 14, 2), new Point(391, 17, 2), new Point(394, 22, 2), new Point(397, 28, 2), new Point(399, 34, 2), new Point(400, 42, 2), new Point(400, 50, 2), new Point(400, 56, 2), new Point(399, 61, 2), new Point(397, 66, 2), new Point(394, 70, 2), new Point(391, 74, 2), new Point(386, 78, 2), new Point(382, 81, 2), new Point(377, 83, 2), new Point(372, 85, 2), new Point(367, 87, 2), new Point(360, 87, 2), new Point(355, 88, 2), new Point(349, 87, 2)
        }, "D"),
        new Gesture(new Point[] {
            new Point(507, 8, 1), new Point(507, 87, 1),
            new Point(513, 7, 2), new Point(528, 7, 2), new Point(537, 8, 2), new Point(544, 10, 2), new Point(550, 12, 2), new Point(555, 15, 2), new Point(558, 18, 2), new Point(560, 22, 2), new Point(561, 27, 2), new Point(562, 33, 2), new Point(561, 37, 2), new Point(559, 42, 2), new Point(556, 45, 2), new Point(550, 48, 2), new Point(544, 51, 2), new Point(538, 53, 2), new Point(532, 54, 2), new Point(525, 55, 2), new Point(519, 55, 2), new Point(513, 55, 2), new Point(510, 55, 2)
        }, "P")
    };

    private static WrapperPoint _wrapperPoint = new WrapperPoint();
    private TcpListener _tcpListener;
    private ActionManager _actionManager;

    private BluetoothListener _bluetoothListener;
    private void Start()
    {
        _actionManager = new ActionManager();
        // _tcpListener = new TcpListener("54321");
        _bluetoothListener = new BluetoothListener();
        
        _bluetoothListener.MessageReceived += OnMessageReceived;
        _bluetoothListener.ClientDisconnected += OnClientDisconnected;
        _bluetoothListener.ClientConnected += OnClientConnected;

        _actionManager.SingleTap += () => Debug.Log("Single Tap");
        _actionManager.PointerRelease += OnPointerRelease;
        _actionManager.DoubleTap += () => Debug.Log("Double Tap");
        _actionManager.LongTap += () => Debug.Log("Long Tap");
        _actionManager.PointerDown += (x, y) => Debug.Log($"Pointer Down ({x}, {y})");
        _actionManager.PointerUp += (x, y) => Debug.Log($"Pointer Up ({x}, {y})");
        _actionManager.PointerMove += (x, y) => Debug.Log($"Pointer Move ({x}, {y})");
        _actionManager.PointerUpdate += (x, y) => Debug.Log($"Pointer Update ({x}, {y})");
        _actionManager.Scroll += delta => Debug.Log($"Scroll ({delta})");
        
        _bluetoothListener.Listen();
    }

    private void OnPointerRelease(List<Point> points)
    {
        var recognized = QRecognizer.Classify(new Gesture(points.ToArray()), _gestures.ToArray());
        Debug.Log($"Recognized class: {recognized.Item1.Name} ({recognized.Item2})");
    }

    private void OnClientDisconnected(string device)
    {
        Debug.Log($"Client {device} disconnected");
        _bluetoothListener.Listen();
        // _tcpListener.Listen();
    }

    private void OnClientConnected(string device)
    {
        Debug.Log($"Client {device} connected");
    }

    private void OnMessageReceived(string message)
    {
        // var messages = message.Split(';');
        // if (messages[0] != "p") return;
        // Debug.Log($"points: {messages[1].Trim()}");
        // JsonUtility.FromJsonOverwrite(messages[1].Trim(), _wrapperPoint);
        // Debug.Log($"json: {_wrapperPoint.points}");
        // var recognized = QRecognizer.Classify(new Gesture(_wrapperPoint.points), _gestures.ToArray());
        // Debug.Log($"Recognized class: {recognized.Item1.Name} ({recognized.Item2})");
        // _actionManager.Feed(message); 
        Debug.Log("Message received: " + message);
    }
}
