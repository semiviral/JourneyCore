using JourneyCoreDisplay.Drawing;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JourneyCoreDisplay
{
    public class WindowManager
    {
        #region VARIABLES

        private readonly RenderWindow _window;

        public bool IsActive => _window.IsOpen;
        public Vector2f ContentScale { get; set; }
        public Vector2f PositionScale { get; set; }
        public List<DrawQueueItem> DrawQueue { get; }

        public event EventHandler<KeyEventArgs> KeyPressed;
        public event EventHandler<KeyEventArgs> KeyReleased;

        #endregion

        public WindowManager(string windowTitle, Vector2f contentScale, float positionScale)
        {
            ContentScale = contentScale;
            PositionScale = ContentScale * positionScale;

            _window = new RenderWindow(new VideoMode(800, 600, 32), windowTitle);
            _window.Closed += OnClose;
            _window.KeyPressed += OnKeyPresed;
            _window.KeyReleased += OnKeyReleased;

            DrawQueue = new List<DrawQueueItem>();
        }

        public void UpdateWindow(float elapsedTime)
        {
            _window.DispatchEvents();
            _window.Clear();

            if (DrawQueue.Count > 0)
            {
                foreach (DrawQueueItem drawItem in DrawQueue.OrderBy(item => item.PriorityLevel))
                {
                    drawItem.Draw(elapsedTime, _window);
                }
            }

            _window.Display();
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
    }
}
