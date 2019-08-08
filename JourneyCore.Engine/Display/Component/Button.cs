using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public class Button : IUiObject, IHoverable, IPressable, IResizeResponsive, Drawable
    {
        private Vector2f _Origin;
        private Vector2f _Position;
        private Vector2f _ResizeFactor;
        private Vector2u _Size;

        public Button(Font defaultFont, string displayedText, bool autoSize, bool respectsCapture)
        {
            AutoSize = autoSize;

            _ResizeFactor = new Vector2f(1f, 1f);
            BackgroundSprite = new Sprite();
            BackgroundShape = new RectangleShape();
            TextObject = new Text();

            Margins = new Margin();
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
            BackgroundShape.Draw(target, states);
            BackgroundSprite.Draw(target, states);
            TextObject.Draw(target, states);
        }

        public bool IsHovered { get; set; }

        public bool RespectsCapture { get; }
        public FloatRect Bounds => AutoSize ? TextObject.GetGlobalBounds() : BackgroundShape.GetGlobalBounds();

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

        public Margin Margins { get; set; }

        public event EventHandler<SizeEventArgs> Resized;

        public IEnumerable<IUiObject> SubscribableObjects()
        {
            return new IUiObject[] { };
        }

        protected void UpdateTextObject(string newDisplayedText)
        {
            Text _newText = new Text(newDisplayedText, DefaultFont);

            TextObject = _newText;

            if (AutoSize)
            {
                FloatRect _localBounds = _newText.GetLocalBounds();

                Size = TextObject.Size;
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
            FloatRect _globalBounds = AutoSize ? TextObject.GetGlobalBounds() : BackgroundShape.GetGlobalBounds();
            _globalBounds.Left *= _ResizeFactor.X;
            _globalBounds.Top *= _ResizeFactor.Y;

            if (_globalBounds.Contains(args.X, args.Y))
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
            size.X += Margins.Left + Margins.Right;
            size.Y += Margins.Top + Margins.Bottom;

            BackgroundSprite.Scale = new Vector2f((float) size.X / BackgroundSprite.TextureRect.Width,
                (float) size.Y / BackgroundSprite.TextureRect.Height);
            BackgroundShape.Size = new Vector2f(size.X, size.Y);

            _Size = size;
        }

        protected void SetPosition(Vector2f position)
        {
            BackgroundShape.Position = position;
            BackgroundSprite.Position = position;
            TextObject.Position = position;

            _Position = position;
        }

        protected void SetOrigin(Vector2f origin)
        {
            BackgroundSprite.Origin = origin;
            BackgroundShape.Origin = origin;
            TextObject.Origin = origin;

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

        protected Text TextObject;
        public Font DefaultFont { get; set; }

        private Color _ForegroundOutlineColor;
        private float _ForegroundOutlineThickness;

        public Color ForegroundOutlineColor
        {
            get => _ForegroundOutlineColor;
            set
            {
                _ForegroundOutlineColor = value;
                TextObject.OutlineColor = value;
            }
        }

        public float ForegroundOutlineThickness
        {
            get => _ForegroundOutlineThickness;
            set
            {
                _ForegroundOutlineThickness = value;
                TextObject.OutlineThickness = value;
            }
        }

        public string DisplayedText
        {
            get => TextObject.DisplayedString;
            set => UpdateTextObject(value);
        }

        public Color ForegroundColor
        {
            get => TextObject.FillColor;
            set => TextObject.FillColor = value;
        }

        #endregion
    }
}