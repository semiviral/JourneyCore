using System;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Display.Interactive
{
    public enum HorizontalTextAlignment
    {
        Left,
        Middle,
        Right
    }

    public enum VerticalTextAlignment
    {
        Top,
        Middle,
        Bottom
    }

    public class Button : Drawable
    {
        private Vector2f _ParsedPosition;
        private Vector2f _ParsedSize;

        public Vector2f Position { get; set; }
        public Vector2f Size { get; set; }

        public Button(Font defaultFont, string text)
        {
            Position = new Vector2f(0f, 0f);
            Size = new Vector2f(10f, 10f);

            BackgroundSprite = new Sprite();
            BackgroundShape = new RectangleShape(Size);
            FillColor = Color.Transparent;
            
            VerticalTextAlignment = VerticalTextAlignment.Top;
            HorizontalTextAlignment = HorizontalTextAlignment.Left;
            DefaultFont = defaultFont;
            Text = text;
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

        private void SizeChanged()
        {
            BackgroundSprite.Scale = new Vector2f(Size.X / BackgroundSprite.TextureRect.Width,
                Size.Y / BackgroundSprite.TextureRect.Height);
            BackgroundShape.Size = Size;

            _ParsedSize = Size;
        }

        #region POSITIONING

        private void PositionChanged()
        {
            BackgroundShape.Position = Position;
            BackgroundSprite.Position = Position;
            CalculateTextPosition();

            _ParsedPosition = Position;
        }

        private void CalculateTextPosition()
        {
            if (Math.Abs(_ParsedPosition.Y - Position.Y) > 0.01)
            {
                switch (VerticalTextAlignment)
                {
                    case VerticalTextAlignment.Top:
                        _TextObject.Position = new Vector2f(_TextObject.Position.X, Position.Y);
                        break;
                    case VerticalTextAlignment.Middle:
                        _TextObject.Position = new Vector2f(_TextObject.Position.X, Position.Y + Size.Y / 2f);
                        break;
                    case VerticalTextAlignment.Bottom:
                        _TextObject.Position = new Vector2f(_TextObject.Position.X,
                            Position.Y + (Size.Y - _TextObject.CharacterSize));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (Math.Abs(_ParsedPosition.X - Position.X) > 0.01)
            {
                switch (HorizontalTextAlignment)
                {
                    case HorizontalTextAlignment.Left:
                        _TextObject.Position = new Vector2f(Position.X, _TextObject.Position.Y);
                        break;
                    case HorizontalTextAlignment.Middle:
                        _TextObject.Position = new Vector2f(Position.X + Size.X / 2f, _TextObject.Position.Y);
                        break;
                    case HorizontalTextAlignment.Right:
                        _TextObject.Position =
                            new Vector2f(Position.X + (Size.X - Text.Length * _TextObject.CharacterSize),
                                _TextObject.Position.Y);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion


        #region VARIABLES - BACKGROUND

        private RectangleShape BackgroundShape { get; }

        public Color FillColor
        {
            get => BackgroundShape.FillColor;
            set => BackgroundShape.FillColor = value;
        }

        public Sprite BackgroundSprite { get; }

        #endregion


        #region VARIABLES - TEXT

        private Text _TextObject;

        public VerticalTextAlignment VerticalTextAlignment { get; set; }
        public HorizontalTextAlignment HorizontalTextAlignment { get; set; }
        public Font DefaultFont { get; }

        public string Text
        {
            get => _TextObject.DisplayedString;
            set => _TextObject = new Text(value, DefaultFont);
        }

        #endregion
    }
}