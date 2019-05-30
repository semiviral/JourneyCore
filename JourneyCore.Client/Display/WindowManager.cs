using System;
using System.Collections.Generic;
using System.Linq;
using JourneyCore.Lib.Graphics.Drawing;
using JourneyCore.Lib.System.Time;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Client.Display
{
    public class WindowManager
    {
        #region VARIABLES

        private RenderWindow Window { get; }
        private SortedList<int, List<DrawItem>> DrawQueue { get; }

        public Vector2u Size => Window.Size;
        public bool IsInMenu { get; private set; }
        public bool IsActive => Window.IsOpen;
        public Vector2f ContentScale { get; set; }
        public Vector2f PositionScale { get; set; }

        public int TargetFps {
            get => _targetFps;
            set {
                // fps changed stuff

                _targetFps = value;
                IndividualFrameTime = 1f / _targetFps;
            }
        }

        private static int _targetFps;
        private static Delta _deltaClock;

        public float ElapsedTime { get; private set; }
        public float IndividualFrameTime { get; private set; }

        #endregion

        public WindowManager(string windowTitle, VideoMode vMode, int targetFps, Vector2f contentScale,
            float positionScale)
        {
            ContentScale = contentScale;
            PositionScale = ContentScale * positionScale;

            Window = new RenderWindow(vMode, windowTitle);
            Window.Closed += OnClose;
            Window.GainedFocus += OnGainedFocus;
            Window.LostFocus += OnLostFocus;
            Window.SetFramerateLimit((uint)targetFps);

            DrawQueue = new SortedList<int, List<DrawItem>>();
            _deltaClock = new Delta();
        }

        public void UpdateWindow()
        {
            DateTime abosluteNow = DateTime.Now;

            ElapsedTime = _deltaClock.GetDelta();

            Window.DispatchEvents();
            Window.Clear();


            if (DrawQueue.Count > 0)
            {
                foreach (List<DrawItem> drawItems in DrawQueue.Values)
                {
                    foreach (DrawItem drawItem in drawItems)
                    {
                        if (drawItem.Lifetime.Ticks != DateTime.MinValue.Ticks &&
                            drawItem.Lifetime.Ticks < abosluteNow.Ticks)
                        {
                            drawItems.Remove(drawItem);
                            continue;
                        }

                        drawItem.Draw(Window, ElapsedTime);
                    }
                }
            }

            Window.Display();
        }

        public void DrawItem(int priority, DrawItem item)
        {
            if (!DrawQueue.Keys.Contains(priority))
            {
                DrawQueue.Add(priority, new List<DrawItem>());
            }

            DrawQueue[priority].Add(item);
        }

        public Vector2i GetRelativeMousePosition()
        {
            return Mouse.GetPosition(Window);
        }


        #region EVENTS

        public event EventHandler Closed;
        public event EventHandler GainedFocus;
        public event EventHandler LostFocus;

        private void OnClose(object sender, EventArgs args)
        {
            Closed?.Invoke(sender, args);

            RenderWindow window = (RenderWindow)sender;
            window.Close();
        }

        public void OnGainedFocus(object sender, EventArgs args)
        {
            GainedFocus?.Invoke(sender, args);
        }

        public void OnLostFocus(object sender, EventArgs args)
        {
            LostFocus?.Invoke(sender, args);
        }

        #endregion

        #region VIEW

        public RenderWindow SetActive(bool activeState)
        {
            Window.SetActive(activeState);

            return Window;
        }

        public View SetView(View view)
        {
            Window.SetView(view);

            return GetView();
        }

        public View GetView()
        {
            return Window.GetView();
        }

        public View SetViewport(FloatRect viewport)
        {
            GetView().Viewport = viewport;

            SetView(GetView());

            return GetView();
        }

        public View MoveView(Vector2f position)
        {
            Window.GetView().Center = position;

            SetView(GetView());

            return GetView();
        }

        public View RotateView(float rotation)
        {
            GetView().Rotation = rotation;

            SetView(GetView());

            return GetView();
        }

        #endregion
    }
}