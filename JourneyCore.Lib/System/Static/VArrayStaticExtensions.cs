using SFML.Graphics;

namespace JourneyCore.Lib.System.Static
{
    public static class StaticExtensions
    {
        public static VertexArray SetOpacity(this VertexArray vArray, byte newOpacity)
        {
            if (newOpacity > 255) newOpacity = 255;

            for (uint i = 0; i < vArray.VertexCount; i++)
            {
                Vertex newVertexAlpha = vArray[i];

                if (newVertexAlpha.Color.A == 0) continue;

                newVertexAlpha.Color.A = newOpacity;

                vArray[i] = newVertexAlpha;
            }

            return vArray;
        }

        public static VertexArray ModifyOpacity(this VertexArray vArray, sbyte alphaModifier, byte minimumAlpha = 0,
            byte maximumAlpha = 255)
        {
            for (uint i = 0; i < vArray.VertexCount; i++)
            {
                Vertex newVertexAlpha = vArray[i];

                if (newVertexAlpha.Color.A == 0) continue;

                if (newVertexAlpha.Color.A + alphaModifier < minimumAlpha)
                    newVertexAlpha.Color.A = minimumAlpha;
                else if (newVertexAlpha.Color.A + alphaModifier > maximumAlpha)
                    newVertexAlpha.Color.A = maximumAlpha;
                else
                    newVertexAlpha.Color.A = (byte) (newVertexAlpha.Color.A + alphaModifier);

                vArray[i] = newVertexAlpha;
            }

            return vArray;
        }
    }
}