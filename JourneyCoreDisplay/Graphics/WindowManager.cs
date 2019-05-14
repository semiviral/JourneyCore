﻿using JourneyCoreLib.Drawing;
using JourneyCoreLib.Time;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JourneyCoreLib
{
    public class WindowManager
    {
        #region VARIABLES

        private readonly RenderWindow _window;

        public bool IsActive => _window.IsOpen;
        public Vector2f ContentScale { get; set; }
        public Vector2f PositionScale { get; set; }
        private List<DrawQueueItem> DrawQueue { get; }

        public static int TargetFps {
            get => _targetFps;
            set {
                // fps changed stuff

                _targetFps = value;
                IndividualFrameTime = 1f / _targetFps;
            }
        }
        private static int _targetFps;
        private static Delta _deltaClock;

        public static float ElapsedTime { get; private set; }
        public static float IndividualFrameTime { get; private set; }
        
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

            DrawQueue = new List<DrawQueueItem>();
            _deltaClock = new Delta();
        }

        public void UpdateWindow()
        {
            ElapsedTime = _deltaClock.GetDelta();

            _window.DispatchEvents();
            _window.Clear();
            _window.PushGLStates();

            if (DrawQueue.Count > 0)
            {
                foreach (DrawQueueItem drawItem in DrawQueue.OrderByDescending(item => item.PriorityLevel))
                {
                    drawItem.Draw(ElapsedTime, _window);
                }
            }

            _window.PopGLStates();
            _window.Display();
        }

        public void DrawPersistent(DrawQueueItem item)
        {
            DrawQueue.Add(item);
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

            return GetView();
        }

        #endregion
    }
}
