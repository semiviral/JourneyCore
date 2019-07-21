using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public class Button : IUIObject, IHoverable, IPressable, IResizeResponsive, Drawable
    {
        protected Vector2f _Origin;
        protected Vector2f _Position;
        protected Vector2f _ResizeFactor;
        protected Vector2u _Size;

        public Button(Font defaultFont, string displayedText, bool autoSize, bool respectsCapture)
        {
            AutoSize = autoSize;

            _ResizeFactor = new Vector2f(1f, 1f);
            BackgroundSprite = new Sprite();
            _BackgroundShape = new RectangleShape();
            _TextObject = new Text();

            Size = new Vector2u(0, 0);
            Position = new Vector2f(0f, 0f);
            Origin = new Vector2f(0f, 0f);

            IsHovered = false;
            IsPressed = false;

            BackgroundColor = Color.Transparent;

            DefaultFont = defaultFont;
            DisplayedText = displayedText;

            Activated = () => true;

            RespectsCapture = respectsCapture;
        }

        public bool AutoSize { get; set; }
        public bool IsPressed { get; set; }

        public Func<bool> Activated { get; set; }

        public void Draw(RenderTarget target, RenderStates states)
        {
            _BackgroundShape.Draw(target, states);
            BackgroundSprite.Draw(target, states);
            _TextObject.Draw(target, states);
        }

        public bool IsHovered { get; set; }

        public bool RespectsCapture { get; }
        public FloatRect Bounds => AutoSize ? _TextObject.GetGlobalBounds() : _BackgroundShape.GetGlobalBounds();

        public Vector2u OriginalParentSize { get; set; }

        public Vector2u Size
        {
            get => _Size;
            set
            {
                SetSize(value);
                Resized?.Invoke(this, new SizeEventArgs(new SizeEvent {Width = _Size.X, Height = _Size.Y}));
            }
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

        public event EventHandler<SizeEventArgs> Resized;

        public IEnumerable<IUIObject> SubscribableObjects()
        {
            return new IUIObject[] { };
        }

        protected void UpdateTextObject(string newDisplayedText)
        {
            Text newText = new Text(newDisplayedText, DefaultFont);

            _TextObject = newText;

            if (AutoSize)
            {
                FloatRect localBounds = newText.GetLocalBounds();

                Size = new Vector2u((uint) (localBounds.Width + localBounds.Left),
                    (uint) (localBounds.Height + localBounds.Top));
                Origin = new Vector2f(Size.X, Size.Y) / 2f;
            }

            Position = _Position;
            ForegroundOutlineColor = _ForegroundOutlineColor;
            ForegroundOutlineThickness = _ForegroundOutlineThickness;
        }


        #region EVENTS

        public event EventHandler<MouseMoveEventArgs> Entered;
        public event EventHandler<MouseMoveEventArgs> Exited;
        public event EventHandler<MouseButtonEventArgs> Pressed;
        public event EventHandler<MouseButtonEventArgs> Released;

        public void OnParentResized(object sender, SizeEventArgs args)
        {
            _ResizeFactor = new Vector2f((float) args.Width / OriginalParentSize.X,
                (float) args.Height / OriginalParentSize.Y);
        }

        public void OnMouseMoved(object sender, MouseMoveEventArgs args)
        {
            FloatRect globalBounds = AutoSize ? _TextObject.GetGlobalBounds() : _BackgroundShape.GetGlobalBounds();
            globalBounds.Left *= _ResizeFactor.X;
            globalBounds.Top *= _ResizeFactor.Y;

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

        public bool OnMousePressed(MouseButtonEventArgs args)
        {
            if (!Activated())
            {
                return false;
            }

            if ((args.Button != Mouse.Button.Left) || !Activated() || !IsHovered)
            {
                return false;
            }

            IsPressed = true;

            Pressed?.Invoke(this, args);

            return true;
        }

        public bool OnMouseReleased(MouseButtonEventArgs args)
        {
            if (!Activated())
            {
                return false;
            }

            if ((args.Button != Mouse.Button.Left) || !Activated() || !IsPressed)
            {
                return false;
            }

            IsPressed = false;

            Released?.Invoke(this, args);

            return true;
        }

        #endregion


        #region POSITIONING / SIZING

        protected void SetSize(Vector2u size)
        {
            BackgroundSprite.Scale = new Vector2f((float) size.X / BackgroundSprite.TextureRect.Width,
                (float) size.Y / BackgroundSprite.TextureRect.Height);
            _BackgroundShape.Size = new Vector2f(size.X, size.Y);

            _Size = size;
        }

        protected void SetPosition(Vector2f position)
        {
            _BackgroundShape.Position = position;
            BackgroundSprite.Position = position;
            _TextObject.Position = position;

            _Position = position;
        }

        protected void SetOrigin(Vector2f origin)
        {
            BackgroundSprite.Origin = origin;
            _BackgroundShape.Origin = origin;
            _TextObject.Origin = origin;

            _Origin = origin;
        }

        #endregion


        #region VARIABLES - BACKGROUND

        protected RectangleShape _BackgroundShape { get; }

        public Color BackgroundColor
        {
            get => _BackgroundShape.FillColor;
            set => _BackgroundShape.FillColor = value;
        }

        public Sprite BackgroundSprite { get; }

        #endregion


        #region VARIABLES - TEXT

        protected Text _TextObject;
        public Font DefaultFont { get; set; }

        protected Color _ForegroundOutlineColor;
        protected float _ForegroundOutlineThickness;

        public Color ForegroundOutlineColor
        {
            get => _ForegroundOutlineColor;
            set
            {
                _ForegroundOutlineColor = value;
                _TextObject.OutlineColor = value;
            }
        }

        public float ForegroundOutlineThickness
        {
            get => _ForegroundOutlineThickness;
            set
            {
                _ForegroundOutlineThickness = value;
                _TextObject.OutlineThickness = value;
            }
        }

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