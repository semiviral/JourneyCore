using SFML.Graphics;

namespace JourneyCore.Lib.Graphics.Drawing
{
    public static class StaticExtensions
    {
        public static VertexArray ModifyOpacity(this VertexArray vArray, sbyte alphaModifier, byte minimumAlpha = 0,
            byte maximumAlpha = 255)
        {
            for (uint i = 0; i < vArray.VertexCount; i++)
            {
                Vertex newVertexAlpha = vArray[i];

                if (newVertexAlpha.Color.A == 0)
                {
                    continue;
                }

                if (newVertexAlpha.Color.A + alphaModifier < minimumAlpha)
                {
                    newVertexAlpha.Color.A = minimumAlpha;
                }
                else if (newVertexAlpha.Color.A + alphaModifier > maximumAlpha)
                {
                    newVertexAlpha.Color.A = maximumAlpha;
                }
                else
                {
                    newVertexAlpha.Color.A = (byte)(newVertexAlpha.Color.A + alphaModifier);
                }

                vArray[i] = newVertexAlpha;
            }

            return vArray;
        }
    }
}