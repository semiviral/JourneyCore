using System;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Game.Context.Entities
{
    public class EntityView
    {
        private readonly View _internalView;

        public EntityView(float centerLeft, float centerTop, float width, float height) : this(
            new Vector2f(centerLeft, centerTop), new Vector2f(width, height))
        {
        }

        public EntityView(Vector2f center, Vector2f size) : this(new View(center, size))
        {
        }

        public EntityView(View view)
        {
            _internalView = view;
        }

        public float Width => _internalView.Viewport.Width;
        public float Height => _internalView.Viewport.Height;

        public event EventHandler<View> PositionChanged;
        public event EventHandler<View> RotationChanged;

        public View GetView()
        {
            return _internalView;
        }

        public View SetPosition(Vector2f center)
        {
            _internalView.Center = center;

            PositionChanged?.Invoke(this, _internalView);

            return _internalView;
        }

        public View SetRotation(float rotation)
        {
            _internalView.Rotation = rotation;

            RotationChanged?.Invoke(this, _internalView);

            return _internalView;
        }

        public View SetViewport(FloatRect viewport)
        {
            _internalView.Viewport = viewport;

            return _internalView;
        }
    }
}