using System;
using System.Collections.Generic;
using System.Linq;
using JourneyCore.Lib.Display.Component;
using JourneyCore.Lib.Display.Drawing;
using JourneyCore.Lib.System.Event.Input;
using JourneyCore.Lib.System.Time;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display
{
    public enum DrawViewLayer
    {
        Game,
        Minimap,
        Ui,
        EscapeMenu,
        Settings
    }

    public class GameWindow
    {
        public const float WidescreenRatio = 16f / 9f;
        public const float LetterboxRatio = 4f / 3f;

        public GameWindow(string windowTitle, VideoMode videoMode, uint targetFps, Vector2f contentScale,
            float positionScale)
        {
            ContentScale = contentScale;
            PositionScale = ContentScale * positionScale;
            TargetFps = targetFps;

            Window = new RenderWindow(videoMode, windowTitle);
            Window.Closed += OnClose;
            Window.Resized += OnResized;
            Window.GainedFocus += OnGainedFocus;
            Window.LostFocus += OnLostFocus;
            Window.MouseWheelScrolled += OnMouseWheelScrolled;
            Window.MouseMoved += OnMouseMoved;
            Window.MouseButtonPressed += OnMouseButtonPressed;
            Window.MouseButtonReleased += OnMouseButtonReleased;
            Window.SetFramerateLimit(TargetFps);

            DrawViews = new SortedList<DrawViewLayer, DrawView>();
            DeltaClock = new Delta();
            ContinuousInputWatcher = new InputWatcher();
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

        public void AddDrawItem(DrawViewLayer layer, int internalLayer, DrawItem drawItem)
        {
            DrawView drawView = DrawViews.SingleOrDefault(view => view.Value.Layer.Equals(layer)).Value;

            drawView?.AddDrawItem(internalLayer, drawItem);
        }


        #region UI OBJECTS

        private List<IUIObject> UIObjects { get; }

        public void SubscribeUiObject(DrawViewLayer layer, int internalLayer, object uiObject)
        {
            if (!(uiObject is IUIObject))
            {
                return;
            }

            if (uiObject is IResizeResponsive resizeResponsiveUiObject)
            {
                SubscribeIResizeResponsive(resizeResponsiveUiObject);
            }

            if (uiObject is IHoverable hoverableUiObject)
            {
                SubscribeIHoverable(hoverableUiObject);
            }

            if (uiObject is IPressable pressableUiObject)
            {
                SubscribeIPressable(pressableUiObject);
            }

            if (uiObject is IScrollable scrollableUiObject)
            {
                SubscribeIScrollable(scrollableUiObject);
            }
        }

        private void SubscribeIResizeResponsive(IResizeResponsive resizeResponsive)
        {
            resizeResponsive.OriginalWindowSize = Window.Size;
            Resized += resizeResponsive.OnParentResized;
        }

        private void SubscribeIHoverable(IHoverable hoverable)
        {
            MouseMoved += hoverable.OnMouseMoved;
        }

        private void SubscribeIPressable(IPressable pressable)
        {
            MouseButtonPressed += (sender, args) =>
            {
                if (pressable.RespectsCapture && PressCaptured)
                {
                    return;
                }

                PressCaptured = pressable.OnMousePressed(args);
            };
            MouseButtonReleased += (sender, args) =>
            {
                if (pressable.RespectsCapture && ReleaseCaptured)
                {
                    return;
                }

                ReleaseCaptured = pressable.OnMouseReleased(args);
            };
        }

        private void SubscribeIScrollable(IScrollable scrollable)
        {
            MouseWheelScrolled += scrollable.OnMouseScrolled;
        }

        #endregion

        #region VARIABLES

        private RenderWindow Window { get; }
        private static Delta DeltaClock { get; set; }
        private SortedList<DrawViewLayer, DrawView> DrawViews { get; }
        private static uint _TargetFps;
        private InputWatcher ContinuousInputWatcher { get; }

        public Vector2u Size => Window.Size;
        public bool IsActive => Window.IsOpen;
        public Vector2f ContentScale { get; set; }
        public Vector2f PositionScale { get; set; }
        public bool PressCaptured { get; set; }
        public bool ReleaseCaptured { get; set; }

        public uint TargetFps
        {
            get => _TargetFps;
            set
            {
                // fps changed stuff

                _TargetFps = value;
                IndividualFrameTime = 1f / _TargetFps;
            }
        }

        public float ElapsedTime { get; private set; }
        public float IndividualFrameTime { get; private set; }

        #endregion


        #region RENDERING

        private void ProcessDrawView(DrawView drawView)
        {
            SetWindowView(drawView.Layer, drawView.View);

            drawView.Draw(Window, ElapsedTime);
        }

        public void UpdateWindow()
        {
            ElapsedTime = DeltaClock.GetDelta();

            Window.DispatchEvents();
            Window.Clear();

            DoFrameUpdate();

            Window.Display();
        }

        private void DoFrameUpdate()
        {
            ContinuousInputWatcher.CheckWatchedInputs();

            foreach ((DrawViewLayer layer, DrawView drawView) in DrawViews.Where(drawView => drawView.Value.Visible))
            {
                ProcessDrawView(drawView);
            }
        }

        #endregion


        #region EVENTS

        public event EventHandler Closed;
        public event EventHandler<SizeEventArgs> Resized;
        public event EventHandler GainedFocus;
        public event EventHandler LostFocus;
        public event EventHandler<MouseWheelScrollEventArgs> MouseWheelScrolled;
        public event EventHandler<MouseMoveEventArgs> MouseMoved;
        public event EventHandler<MouseButtonEventArgs> MouseButtonPressed;
        public event EventHandler<MouseButtonEventArgs> MouseButtonReleased;

        private void OnClose(object sender, EventArgs args)
        {
            Closed?.Invoke(sender, args);

            RenderWindow window = (RenderWindow)sender;
            window.Close();
        }

        private void OnResized(object sender, SizeEventArgs args)
        {
            Resized?.Invoke(sender, args);
        }

        public void OnGainedFocus(object sender, EventArgs args)
        {
            GainedFocus?.Invoke(sender, args);
        }

        public void OnLostFocus(object sender, EventArgs args)
        {
            LostFocus?.Invoke(sender, args);
        }

        public void OnMouseWheelScrolled(object sender, MouseWheelScrollEventArgs args)
        {
            MouseWheelScrolled?.Invoke(sender, args);
        }

        public void OnMouseMoved(object sender, MouseMoveEventArgs args)
        {
            MouseMoved?.Invoke(sender, args);
        }

        public void OnMouseButtonPressed(object sender, MouseButtonEventArgs args)
        {
            PressCaptured = false;
            ReleaseCaptured = false;

            MouseButtonPressed?.Invoke(sender, args);
        }

        public void OnMouseButtonReleased(object sender, MouseButtonEventArgs args)
        {
            MouseButtonReleased?.Invoke(sender, args);
        }

        #endregion


        #region VIEW

        public DrawView CreateDrawView(DrawView drawView)
        {
            if (DrawViews.Any(dView => dView.Value.Layer.Equals(drawView.Layer)))
            {
                return null;
            }

            DrawViews.Add(drawView.Layer, drawView);

            return drawView;
        }

        public View SetWindowView(DrawViewLayer layer, View view)
        {
            Window.SetView(view);

            return GetDrawView(layer)?.View;
        }

        public DrawView GetDrawView(DrawViewLayer layer)
        {
            return DrawViews.SingleOrDefault(view => view.Value.Layer.Equals(layer)).Value;
        }

        public DrawView SetViewport(DrawViewLayer layer, FloatRect viewport)
        {
            DrawView drawView = GetDrawView(layer);

            drawView.View.Viewport = viewport;

            SetWindowView(layer, drawView.View);

            return drawView;
        }

        public DrawView MoveView(DrawViewLayer layer, Vector2f position)
        {
            DrawView drawView = GetDrawView(layer);

            drawView.Position = position;

            SetWindowView(layer, drawView.View);

            return drawView;
        }

        public DrawView RotateView(DrawViewLayer layer, float rotation)
        {
            DrawView drawView = GetDrawView(layer);

            drawView.Rotation = rotation;

            SetWindowView(layer, drawView.View);

            return drawView;
        }

        #endregion


        #region INPUTWATCHER

        public void AddWatchedInput(Keyboard.Key key, Action inputAction, Func<bool> enabledCheck = null,
            bool singlePress = false)
        {
            ContinuousInputWatcher.AddWatchedInput(key, inputAction, enabledCheck, singlePress);
        }

        public void AddWatchedInput(Mouse.Button button, Action inputAction, bool respectsCapture, Func<bool> enabledCheck = null,
            bool singlePress = false)
        {
            ContinuousInputWatcher.AddWatchedInput(button, () =>
            {
                if (respectsCapture && PressCaptured)
                {
                    return;
                }

                inputAction();
            }, enabledCheck, singlePress);
        }

        #endregion
    }
}