using System;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public class Button : Drawable
    {
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

        public Button(GameWindow windowContext, Font defaultFont, string displayedText)
        {
            windowContext.MouseMoved += OnMouseMoved;
            windowContext.MouseButtonPressed += OnMouseButtonPressed;
            windowContext.MouseButtonReleased += OnMouseButtonReleased;

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

        public event EventHandler<MouseMoveEventArgs> MouseEntered;
        public event EventHandler<MouseMoveEventArgs> MouseExited;
        public event EventHandler<MouseButtonEventArgs> Pressed;
        public event EventHandler<MouseButtonEventArgs> Released;

        private void OnMouseMoved(object sender, MouseMoveEventArgs args)
        {
            if (BackgroundShape.GetGlobalBounds().Contains(args.X, args.Y))
            {
                IsHovered = true;

                MouseEntered?.Invoke(sender, args);
            }
            else
            {
                if (!IsHovered)
                {
                    return;
                }

                IsHovered = false;

                MouseExited?.Invoke(sender, args);
            }
        }

        private void OnMouseButtonPressed(object sender, MouseButtonEventArgs args)
        {
            if (!IsHovered)
            {
                return;
            }

            IsPressed = true;

            Pressed?.Invoke(sender, args);
        }

        private void OnMouseButtonReleased(object sender, MouseButtonEventArgs args)
        {
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