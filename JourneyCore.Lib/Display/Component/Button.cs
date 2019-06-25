using System;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Color = SFML.Graphics.Color;

namespace JourneyCore.Lib.Display.Component
{
    public class Button : IUIObject, IHoverable, IPressable, Drawable
    {
        private Vector2f OriginalWindowSize { get; set; }
        private Vector2f ResizeFactor { get; set; }

        private Vector2f _Origin;
        private Vector2f _Position;
        private Vector2f _Size;

        public Vector2f Size
        {
            get => _Size;
            set => SetSize(value);
        }

        public Vector2f Position
        {
            get => _Position;
            set => SetPosition(value);
        }

        public Vector2f Origin
        {
            get => _Origin;
            set => SetOrigin(value);
        }

        public bool IsPressed { get; set; }
        public bool IsHovered { get; set; }

        public Func<bool> Activated { get; set; }

        public Button(Font defaultFont, string displayedText)
        {
            ResizeFactor = new Vector2f(1f, 1f);
            BackgroundSprite = new Sprite();
            BackgroundShape = new RectangleShape();
            _TextObject = new Text();

            Size = new Vector2f(10f, 10f);
            Position = new Vector2f(0f, 0f);
            Origin = new Vector2f(0f, 0f);

            IsHovered = false;
            IsPressed = false;

            BackgroundColor = Color.Transparent;

            DefaultFont = defaultFont;
            DisplayedText = displayedText;

            Activated = () => true;
        }

        public void SubscribeObject(GameWindow window)
        {
            OriginalWindowSize = new Vector2f(window.Size.X, window.Size.Y);
            window.Resized += OnWindowResized;
            window.MouseMoved += OnMouseMoved;
            window.MouseButtonPressed += OnMousePressed;
            window.MouseButtonReleased += OnMouseReleased;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            BackgroundShape.Draw(target, states);
            BackgroundSprite.Draw(target, states);
            _TextObject.Draw(target, states);
        }

        private void UpdateTextObject(string newDisplayedText)
        {
            Text newText = new Text(newDisplayedText, DefaultFont);

            FloatRect localBounds = newText.GetLocalBounds();
            newText.Origin = new Vector2f(localBounds.Width / 2f + localBounds.Left,
                localBounds.Height / 2f + localBounds.Top);

            _TextObject = newText;
        }

        #region EVENTS

        public void OnWindowResized(object sender, SizeEventArgs args)
        {
            ResizeFactor = new Vector2f(args.Width / OriginalWindowSize.X, args.Height / OriginalWindowSize.Y);
        }

        public event EventHandler<MouseMoveEventArgs> Entered;
        public event EventHandler<MouseMoveEventArgs> Exited;
        public event EventHandler<MouseButtonEventArgs> Pressed;
        public event EventHandler<MouseButtonEventArgs> Released;

        public void OnMouseMoved(object sender, MouseMoveEventArgs args)
        {
            if (!Activated())
            {
                return;
            }

            FloatRect globalBounds = BackgroundShape.GetGlobalBounds();
            globalBounds.Left *= ResizeFactor.X;
            globalBounds.Top *= ResizeFactor.Y;

            if (globalBounds.Contains(args.X, args.Y))
            {
                IsHovered = true;

                Entered?.Invoke(sender, args);
            }
            else
            {
                if (!IsHovered)
                {
                    return;
                }

                IsHovered = false;

                Exited?.Invoke(sender, args);
            }
        }

        public void OnMousePressed(object sender, MouseButtonEventArgs args)
        {
            if (!Activated())
            {
                return;
            }

            if (!IsHovered)
            {
                return;
            }

            IsPressed = true;

            Pressed?.Invoke(sender, args);
        }

        public void OnMouseReleased(object sender, MouseButtonEventArgs args)
        {
            if (!Activated())
            {
                return;
            }

            if (!IsPressed)
            {
                return;
            }

            IsPressed = false;

            Released?.Invoke(sender, args);
        }

        #endregion

        #region POSITIONING / SIZING

        private void SetSize(Vector2f size)
        {
            BackgroundSprite.Scale = new Vector2f(size.X / BackgroundSprite.TextureRect.Width,
                size.Y / BackgroundSprite.TextureRect.Height);
            BackgroundShape.Size = size;

            _Size = size;
        }

        private void SetPosition(Vector2f position)
        {
            BackgroundShape.Position = position;
            BackgroundSprite.Position = position;
            _TextObject.Position = position + Size / 2f;

            _Position = position;
        }

        private void SetOrigin(Vector2f origin)
        {
            BackgroundSprite.Origin = origin;
            BackgroundShape.Origin = origin;

            _Origin = origin;
        }

        #endregion


        #region VARIABLES - BACKGROUND

        private RectangleShape BackgroundShape { get; }

        public Color BackgroundColor
        {
            get => BackgroundShape.FillColor;
            set => BackgroundShape.FillColor = value;
        }

        public Sprite BackgroundSprite { get; }

        #endregion


        #region VARIABLES - TEXT

        private Text _TextObject;
        public Font DefaultFont { get; set; }

        public string DisplayedText
        {
            get => _TextObject.DisplayedString;
            set => UpdateTextObject(value);
        }

        public Color ForegroundColor
        {
            get => _TextObject.FillColor;
            set => _TextObject.FillColor = value;
        }

        #endregion
    }
}