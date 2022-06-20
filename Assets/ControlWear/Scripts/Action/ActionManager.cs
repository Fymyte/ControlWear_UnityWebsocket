using System;
using System.Collections.Generic;
using Ohrizon.ControlWear.DollarQ;
using UnityEngine;

namespace Ohrizon.ControlWear.Action
{
    public class ActionManager
    {
        public event System.Action SingleTap;
        public event System.Action DoubleTap;
        public event System.Action LongTap;
        public event System.Action<float> Scroll;
        public event System.Action<int, int> PointerDown;
        public event System.Action<int, int> PointerUpdate;
        public event System.Action<int, int> PointerMove;
        public event System.Action<int, int> PointerUp;
        public event System.Action<List<Point>> PointerRelease;

        [Serializable]
        private class WrapperPoint
        {
            public Point[] points;
        }

        public void Feed(string message)
        {
            var parts = message.Split(';');
            switch (parts[0])
            {
                case "s":
                   SingleTap?.Invoke(); 
                    break;
                case "d":
                    DoubleTap?.Invoke();
                    break;
                case "l":
                    LongTap?.Invoke();
                    break;
                case "o":
                    Scroll?.Invoke(float.Parse(parts[1].Trim()));
                    break;
                case "a":
                    PointerDown?.Invoke(int.Parse(parts[1].Trim()), int.Parse(parts[2]));
                    break;
                case "u":
                    PointerUpdate?.Invoke(int.Parse(parts[1].Trim()), int.Parse(parts[2].Trim()));
                    break;
                case "m":
                    PointerMove?.Invoke(int.Parse(parts[1].Trim()), int.Parse(parts[2].Trim()));
                    break;
                case "r":
                    PointerUp?.Invoke(int.Parse(parts[1].Trim()), int.Parse(parts[2].Trim()));
                    break;
                case "p":
                    var points = JsonUtility.FromJson<ConnectionManager.WrapperPoint>(parts[1].Trim());
                    PointerRelease?.Invoke(new List<Point>(points.points));
                    break;
                default:
                    Debug.LogWarning($"Action not recognized: {message}");
                    break;
            }
        }
    }
}