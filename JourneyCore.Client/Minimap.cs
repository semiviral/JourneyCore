using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Client
{
    public class Minimap
    {
        public Minimap()
        {
            VArray = new VertexArray(PrimitiveType.Quads);
        }

        public Minimap(Transformable anchorExpression) : this()
        {
            AnchorExpression = anchorExpression;
        }

        public VertexArray VArray { get; }
        public Transformable AnchorExpression { get; set; }

        public Task AnchorPositionChanged(object sender, Vector2f position)
        {
            AnchorExpression.Position = position;

            return Task.CompletedTask;
        }

        public Task AnchorRotationChanged(object sender, float rotation)
        {
            AnchorExpression.Rotation = rotation;

            return Task.CompletedTask;
        }
    }
}