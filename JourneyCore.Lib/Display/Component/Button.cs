using System;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public class Button : Drawable
    {
        private Vector2f _ParsedPosition;
        private Vector2f _ParsedSize;

        public Vector2f Position { get; set; }
        public Vector2f Size { get; set; }

        public bool IsPressed { get; set; }
        public bool IsHovered { get; set; }

        public Color DefaultColor { get; set; }
        public Color HoverColor { get; set; }
        public Color PressedColor { get; set; }

        public Button(GameWindow windowContext, Font defaultFont, string text)
        {
            windowContext.MouseMoved += OnMouseMoved;
            windowContext.MouseButtonPressed += OnMouseButtonPressed;
            windowContext.MouseButtonReleased += OnMouseButtonReleased;

            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;
            Pressed += OnPressed;
            Released += OnReleased;
        
            Position = new Vector2f(0f, 0f);
            Size = new Vector2f(10f, 10f);

            IsHovered = false;
            IsPressed = false;

            BackgroundSprite = new Sprite();
            BackgroundShape = new RectangleShape(Size);
            FillColor = Color.Transparent;

            DefaultFont = defaultFont;
            Text = text;
            FloatRect localBounds = _TextObject.GetLocalBounds();
            _TextObject.Origin = new Vector2f(localBounds.Width / 2f, localBounds.Height / 2f);

        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            if (_ParsedSize != Size)
            {
                SizeChanged();
            }

            if (_ParsedPosition != Position)
            {
                PositionChanged();
            }

            BackgroundShape.Draw(target, states);
            BackgroundSprite.Draw(target, states);
            _TextObject.Draw(target, states);
        }

        private void OnMouseMoved(object sender, MouseMoveEventArgs args)
        {
            if (BackgroundShape.GetGlobalBounds().Contains(args.X, args.Y))
            {
                IsHovered = true;

                MouseEntered?.Invoke(sender, args);
            }
            else
            {
                if (IsHovered)
                {
                    MouseExited?.Invoke(sender, args);
                }

                IsHovered = false;
            }
        }

        private void OnMouseButtonPressed(object sender, MouseButtonEventArgs args)
        {
            if (!IsHovered)
            {
                return;
            }

            IsPressed = true;

            FillColor = PressedColor;

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

        #region EVENTS

        public event EventHandler<MouseMoveEventArgs> MouseEntered;
        public event EventHandler<MouseMoveEventArgs> MouseExited;
        public event EventHandler<MouseButtonEventArgs> Pressed;
        public event EventHandler<MouseButtonEventArgs> Released;

        #endregion

        #region AUTO-COLORING

        private void OnMouseEntered(object sender, MouseMoveEventArgs args)
        {
            FillColor = IsPressed ? PressedColor : HoverColor;
        }

        private void OnMouseExited(object sender, MouseMoveEventArgs args)
        {
            if (IsHovered && !IsPressed)
            {
                FillColor = DefaultColor;
            }
        }

        private void OnPressed(object sender, MouseButtonEventArgs args)
        {
            FillColor = PressedColor;
        }

        private void OnReleased(object sender, MouseButtonEventArgs args)
        {
            FillColor = IsHovered ? HoverColor : DefaultColor;
        }

        #endregion

        #region POSITIONING / SIZING

        private void SizeChanged()
        {
            BackgroundSprite.Scale = new Vector2f(Size.X / BackgroundSprite.TextureRect.Width,
                Size.Y / BackgroundSprite.TextureRect.Height);
            BackgroundShape.Size = Size;

            _ParsedSize = Size;
        }

        private void PositionChanged()
        {
            BackgroundShape.Position = Position;
            BackgroundSprite.Position = Position;

            _TextObject.Position = Position + (Size / 2f);

            _ParsedPosition = Position;
        }

        #endregion


        #region VARIABLES - BACKGROUND

        private RectangleShape BackgroundShape { get; }

        public Color FillColor {
            get => BackgroundShape.FillColor;
            set => BackgroundShape.FillColor = value;
        }

        public Sprite BackgroundSprite { get; }

        #endregion


        #region VARIABLES - TEXT

        private Text _TextObject;
        public Font DefaultFont { get; }

        public string Text {
            get => _TextObject.DisplayedString;
            set => _TextObject = new Text(value, DefaultFont);
        }

        #endregion
    }
}