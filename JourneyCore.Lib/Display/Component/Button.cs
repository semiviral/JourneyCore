using System;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public class Button : IUIObject, IHoverable, IPressable, IResizeResponsive, Drawable
    {
        protected Vector2f _Origin;
        protected Vector2f _Position;
        protected Vector2f _Size;
        protected Vector2f ResizeFactor { get; set; }

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

        public Vector2u OriginalWindowSize { get; set; }
        public bool AutoSize { get; set; }
        public bool IsPressed { get; set; }

        public Func<bool> Activated { get; set; }

        public Button(Font defaultFont, string displayedText, bool autoSize)
        {
            AutoSize = autoSize;

            ResizeFactor = new Vector2f(1f, 1f);
            BackgroundSprite = new Sprite();
            BackgroundShape = new RectangleShape();
            _TextObject = new Text();

            Size = new Vector2f(0f, 0f);
            Position = new Vector2f(0f, 0f);
            Origin = new Vector2f(0f, 0f);

            IsHovered = false;
            IsPressed = false;

            BackgroundColor = Color.Transparent;

            DefaultFont = defaultFont;
            DisplayedText = displayedText;

            Activated = () => true;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            BackgroundShape.Draw(target, states);
            BackgroundSprite.Draw(target, states);
            _TextObject.Draw(target, states);
        }

        public bool IsHovered { get; set; }

        protected void UpdateTextObject(string newDisplayedText)
        {
            Text newText = new Text(newDisplayedText, DefaultFont);

            FloatRect localBounds = newText.GetLocalBounds();
            newText.Origin = new Vector2f(localBounds.Width / 2f + localBounds.Left,
                localBounds.Height / 2f + localBounds.Top);

            _TextObject = newText;

            if (AutoSize)
            {
                SetSize(new Vector2f(localBounds.Width, localBounds.Height));
            }
        }


        #region EVENTS

        public event EventHandler<MouseMoveEventArgs> Entered;
        public event EventHandler<MouseMoveEventArgs> Exited;
        public event EventHandler<MouseButtonEventArgs> Pressed;
        public event EventHandler<MouseButtonEventArgs> Released;

        public void OnParentResized(object sender, SizeEventArgs args)
        {
            ResizeFactor = new Vector2f((float)args.Width / OriginalWindowSize.X, (float)args.Height / OriginalWindowSize.Y);
        }

        public void OnMouseMoved(object sender, MouseMoveEventArgs args)
        {
            if (!Activated())
            {
                return;
            }

            FloatRect globalBounds = AutoSize ? _TextObject.GetGlobalBounds() : BackgroundShape.GetGlobalBounds();
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

        protected void SetSize(Vector2f size)
        {
            BackgroundSprite.Scale = new Vector2f(size.X / BackgroundSprite.TextureRect.Width,
                size.Y / BackgroundSprite.TextureRect.Height);
            BackgroundShape.Size = size;

            _Size = size;
        }

        protected void SetPosition(Vector2f position)
        {
            BackgroundShape.Position = position;
            BackgroundSprite.Position = position;
            _TextObject.Position = position + Size / 2f;

            _Position = position;
        }

        protected void SetOrigin(Vector2f origin)
        {
            BackgroundSprite.Origin = origin;
            BackgroundShape.Origin = origin;

            _Origin = origin;
        }

        #endregion


        #region VARIABLES - BACKGROUND

        protected RectangleShape BackgroundShape { get; }

        public Color BackgroundColor
        {
            get => BackgroundShape.FillColor;
            set => BackgroundShape.FillColor = value;
        }

        public Sprite BackgroundSprite { get; }

        #endregion


        #region VARIABLES - TEXT

        protected Text _TextObject;
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