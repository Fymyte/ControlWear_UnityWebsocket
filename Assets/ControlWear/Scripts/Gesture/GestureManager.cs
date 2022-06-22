using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Ohrizon.ControlWear.DollarQ;
using UnityEngine;

namespace Ohrizon.ControlWear.Gesture
{
    public class GestureManager
    {
        #region Events

        /// <summary>
        /// Single tap on watch face
        /// </summary>
        public event Action SingleTap;
        /// <summary>
        /// Double tap on watch face
        /// </summary>
        public event Action DoubleTap;
        /// <summary>
        /// Long tap on watch face
        /// </summary>
        public event Action LongTap;
        /// <summary>
        /// Scroll nub used
        /// <param name="delta">amount scrolled</param>
        /// </summary>
        public event Action<float> Scroll;
        /// <summary>
        /// Watch face touched
        /// <param name="X">X coordinate of touched input</param>
        /// <param name="Y">Y coordinate of touched input</param>
        /// </summary>
        public event Action<int, int> PointerDown;
        public event Action<int, int> PointerUpdate;
        public event Action<int, int> PointerMove;
        public event Action<int, int> PointerUp;
        public event Action<string, float> PointerRelease;

        #endregion

        #region Constants

        private const int maxDistanceForGestureRecognition = 50; 

        #endregion

        #region Known gestures

        [Serializable]
        private class GestureListWrapper
        {
            public List<DollarQ.Gesture> gestures = new List<DollarQ.Gesture>();
        }
        
        private static List<DollarQ.Gesture> _knownGestures = new List<DollarQ.Gesture>()
        {
            new DollarQ.Gesture(new Point[]
            {
               new Point(30, 20, 1), new Point(200, 20, 1)
            }, "Straight line"),
        };
        public static ReadOnlyCollection<DollarQ.Gesture> KnownGestures => _knownGestures.AsReadOnly();

        public static string ExportToJson()
        {
            return JsonUtility.ToJson(new GestureListWrapper() { gestures = _knownGestures});
        }

        public static void LoadFromJson(string json)
        {
            GestureListWrapper wrapper = JsonUtility.FromJson<GestureListWrapper>(json);
            _knownGestures.Clear();
            _knownGestures = wrapper.gestures;
        }

        #endregion

        #region Gesture recognition

        [Serializable]
        private class WrapperPoint
        {
            public List<Point> points;
        }

        public void Recognize(string message)
        {
            void NotRecognized() => Debug.LogWarning("Action not recognized: " + message);
            // Parsing of floats and integers using culture invariant standard. (ex: dot for decimal, space for thousands..)
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
                    Scroll?.Invoke(float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture));
                    break;
                case "a":
                    PointerDown?.Invoke(int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture), int.Parse(parts[2].Trim(), CultureInfo.InvariantCulture));
                    break;
                case "u":
                    PointerUpdate?.Invoke(int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture), int.Parse(parts[2].Trim(), CultureInfo.InvariantCulture));
                    break;
                case "m":
                    PointerMove?.Invoke(int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture), int.Parse(parts[2].Trim(), CultureInfo.InvariantCulture));
                    break;
                case "r":
                    PointerUp?.Invoke(int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture), int.Parse(parts[2].Trim(), CultureInfo.InvariantCulture));
                    break;
                case "p":
                    var points = JsonUtility.FromJson<WrapperPoint>(parts[1].Trim()).points;
                    var recognizedGesture = QRecognizer.Classify(new DollarQ.Gesture(points.ToArray()), _knownGestures.ToArray());
                    PointerRelease?.Invoke(recognizedGesture.Gesture.Name, recognizedGesture.Distance);
                    
                    if (recognizedGesture.Distance > maxDistanceForGestureRecognition)
                        NotRecognized();
                    else
                    {
                    }
                    break;
                default:
                    NotRecognized();
                    break;
            }
            
        }
        
        #endregion
    }
}