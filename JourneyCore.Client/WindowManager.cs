using JourneyCore.Lib.Graphics.Drawing;
using JourneyCore.Lib.System.Time;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JourneyCore.Client
{
    public class WindowManager
    {
        #region VARIABLES

        private readonly RenderWindow _window;
        private readonly List<DrawQueueItem> _drawQueue;

        public Vector2u Size => _window.Size;

        public bool IsInMenu { get; private set; }
        public bool IsActive => _window.IsOpen;
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

        public event EventHandler<KeyEventArgs> KeyPressed;
        public event EventHandler<KeyEventArgs> KeyReleased;

        #endregion

        public WindowManager(string windowTitle, VideoMode vMode, int targetFps, Vector2f contentScale, float positionScale)
        {
            ContentScale = contentScale;
            PositionScale = ContentScale * positionScale;

            _window = new RenderWindow(vMode, windowTitle);
            _window.Closed += OnClose;
            _window.KeyPressed += OnKeyPresed;
            _window.KeyReleased += OnKeyReleased;
            _window.SetFramerateLimit((uint)targetFps);

            _drawQueue = new List<DrawQueueItem>();
            _deltaClock = new Delta();
        }

        public void UpdateWindow()
        {
            DateTime abosluteNow = DateTime.Now;

            ElapsedTime = _deltaClock.GetDelta();

            _window.DispatchEvents();
            _window.Clear();
            _window.PushGLStates();

            if (_drawQueue.Count > 0)
            {
                foreach (DrawQueueItem drawItem in _drawQueue.OrderByDescending(item => item.PriorityLevel))
                {
                    if (drawItem.Lifetime.Ticks < abosluteNow.Ticks && drawItem.Lifetime.Ticks != DateTime.MinValue.Ticks)
                    {
                        _drawQueue.Remove(drawItem);
                        continue;
                    }

                    drawItem.Draw(ElapsedTime, _window);
                }
            }

            _window.PopGLStates();
            _window.Display();
        }

        public void DrawItem(DrawQueueItem item)
        {
            _drawQueue.Add(item);
        }

        #region EVENTS

        private void OnKeyPresed(object sender, KeyEventArgs args)
        {
            KeyPressed?.Invoke(sender, args);
        }

        private void OnKeyReleased(object sender, KeyEventArgs args)
        {
            KeyReleased?.Invoke(sender, args);
        }

        private void OnClose(object sender, EventArgs args)
        {
            RenderWindow window = (RenderWindow)sender;
            window.Close();
        }

        #endregion

        #region VIEW

        public View SetView(View view)
        {
            _window.SetView(view);

            return GetView();
        }

        public View GetView()
        {
            return _window.GetView();
        }

        public View SetViewport(FloatRect viewport)
        {
            GetView().Viewport = viewport;

            SetView(GetView());

            return GetView();
        }

        public View MoveView(Vector2f position)
        {
            _window.GetView().Center = position;

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



        public Vector2i GetRelativeMousePosition()
        {
            return Mouse.GetPosition(_window);
        }
    }
}
