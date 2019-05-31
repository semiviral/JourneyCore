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
        public WindowManager(string windowTitle, VideoMode videoMode, int targetFps, Vector2f contentScale,
            float positionScale)
        {
            ContentScale = contentScale;
            PositionScale = ContentScale * positionScale;
            TargetFps = targetFps;

            Window = new RenderWindow(videoMode, windowTitle);
            Window.Closed += OnClose;
            Window.GainedFocus += OnGainedFocus;
            Window.LostFocus += OnLostFocus;
            Window.SetFramerateLimit((uint)TargetFps);

            DrawViews = new List<DrawView>();
            DeltaClock = new Delta();
        }

        public RenderWindow SetActive(bool activeState)
        {
            Window.SetActive(activeState);

            return Window;
        }

        public Vector2i GetRelativeMousePosition()
        {
            return Mouse.GetPosition(Window);
        }

        #region VARIABLES

        private RenderWindow Window { get; }
        private static Delta DeltaClock { get; set; }
        private List<DrawView> DrawViews { get; }
        private static int _targetFps;

        public Vector2u Size => Window.Size;
        public bool IsInMenu { get; private set; }
        public bool IsActive => Window.IsOpen;
        public Vector2f ContentScale { get; set; }
        public Vector2f PositionScale { get; set; }

        public int TargetFps
        {
            get => _targetFps;
            set
            {
                // fps changed stuff

                _targetFps = value;
                IndividualFrameTime = 1f / _targetFps;
            }
        }

        public float ElapsedTime { get; private set; }
        public float IndividualFrameTime { get; private set; }

        #endregion


        #region RENDERING

        public void UpdateWindow()
        {
            ElapsedTime = DeltaClock.GetDelta();

            Window.DispatchEvents();
            Window.Clear();

            foreach (DrawView drawView in DrawViews)
            {
                SetWindowView("game", drawView.View);

                drawView.Draw(Window, ElapsedTime);
            }

            Window.Display();
        }

        public void DrawItem(string viewName, int priority, DrawItem drawItem)
        {
            DrawView drawView = DrawViews.SingleOrDefault(view => view.Name.Equals(viewName));

            drawView?.AddDrawItem(priority, drawItem);
        }

        #endregion


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

        public DrawView CreateView(string viewName, View defaultView)
        {
            return CreateView(new DrawView(viewName, defaultView));
        }

        public DrawView CreateView(DrawView drawView)
        {
            if (DrawViews.Any(dView => dView.Name.Equals(drawView.Name)))
            {
                return null;
            }

            DrawViews.Add(drawView);

            return drawView;
        }

        public View SetWindowView(string name, View view)
        {
            Window.SetView(view);

            return GetView(name);
        }

        public View GetView(string name)
        {
            return DrawViews.SingleOrDefault(view => view.Name.Equals(name))?.View;
        }

        public View SetViewport(string name, FloatRect viewport)
        {
            View view = GetView(name);

            view.Viewport = viewport;

            SetWindowView(name, view);

            return view;
        }

        public View MoveView(string name, Vector2f position)
        {
            View view = GetView(name);

            view.Center = position;

            SetWindowView(name, view);

            return view;
        }

        public View RotateView(string name, float rotation)
        {
            View view = GetView(name);

            view.Rotation = rotation;

            SetWindowView(name, view);

            return view;
        }

        #endregion
    }
}