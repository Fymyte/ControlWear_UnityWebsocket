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
        public event Action ArrowLeft;
        public event Action ArrowRight;
        public event Action ArrowUp;
        public event Action ArrowDown;
        public event Action HorizontalLeft;
        public event Action HorizontalRight;
        public event Action VerticalUp;
        public event Action VerticalDown;

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

        #region Recognized gestures names

        private const string LINE_HORIZONTAL = "horizontal_line";
        private const string LINE_VERTICAL = "vertical_line";
        private const string ARROW_RIGHT = "right_arrow";
        private const string ARROW_LEFT = "left_arrow";
        private const string ARROW_UP = "up_arrow";
        private const string ARROW_DOWN = "down_arrow";

        #endregion
        
        private static List<DollarQ.Gesture> _knownGestures = new List<DollarQ.Gesture>()
        {
            new DollarQ.Gesture(new[]
            {
               new Point(20, 232, 1), new Point(484, 232, 1)
            }, LINE_HORIZONTAL),
            new DollarQ.Gesture(new[]
            {
               new Point(232, 20, 1), new Point(232, 484, 1)
            }, LINE_VERTICAL),
            new DollarQ.Gesture(new[]
            {
               new Point(100, 484, 1), new Point(484, 232, 1), new Point(100, 20, 1)
            }, ARROW_RIGHT),
            new DollarQ.Gesture(new[]
            {
               new Point(404, 484, 1), new Point(20, 232, 1), new Point(404, 20, 1)
            }, ARROW_LEFT),
            new DollarQ.Gesture(new[]
            {
               new Point(484, 404, 1), new Point(232, 20, 1), new Point(20, 404, 1)
            }, ARROW_UP),
            new DollarQ.Gesture(new[]
            {
               new Point(484, 100, 1), new Point(232, 404, 1), new Point(100, 100, 1)
            }, ARROW_DOWN),
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
                        switch (recognizedGesture.Gesture.Name)
                        {
                            case ARROW_UP:
                                ArrowUp?.Invoke();
                                break;
                            case ARROW_DOWN:
                                ArrowDown?.Invoke();
                                break;
                            case ARROW_LEFT:
                                ArrowLeft?.Invoke();
                                break;
                            case ARROW_RIGHT:
                                ArrowRight?.Invoke();
                                break;
                            case LINE_HORIZONTAL:
                                Debug.Log("Horizontal: " + JsonUtility.ToJson(points));
                                if (points[0].X > points[points.Count - 1].X)
                                    HorizontalLeft?.Invoke(); 
                                else
                                    HorizontalRight?.Invoke();
                                break;
                            case LINE_VERTICAL:
                                Debug.Log("Vertical: " + JsonUtility.ToJson(points));
                                if (points[0].Y > points[points.Count - 1].Y)
                                    VerticalUp?.Invoke();
                                else
                                    VerticalDown?.Invoke();
                                break;
                        }
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