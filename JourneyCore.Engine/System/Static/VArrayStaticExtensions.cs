using SFML.Graphics;

namespace JourneyCore.Lib.System.Static
{
    public static class StaticExtensions
    {
        public static VertexArray SetOpacity(this VertexArray vArray, byte newOpacity)
        {
            if (newOpacity > 255)
            {
                newOpacity = 255;
            }

            for (uint _i = 0; _i < vArray.VertexCount; _i++)
            {
                Vertex _newVertexAlpha = vArray[_i];

                if (_newVertexAlpha.Color.A == 0)
                {
                    continue;
                }

                _newVertexAlpha.Color.A = newOpacity;

                vArray[_i] = _newVertexAlpha;
            }

            return vArray;
        }

        public static VertexArray ModifyOpacity(this VertexArray vArray, sbyte alphaModifier, byte minimumAlpha = 0,
            byte maximumAlpha = 255)
        {
            for (uint _i = 0; _i < vArray.VertexCount; _i++)
            {
                Vertex _newVertexAlpha = vArray[_i];

                if (_newVertexAlpha.Color.A == 0)
                {
                    continue;
                }

                if ((_newVertexAlpha.Color.A + alphaModifier) < minimumAlpha)
                {
                    _newVertexAlpha.Color.A = minimumAlpha;
                }
                else if ((_newVertexAlpha.Color.A + alphaModifier) > maximumAlpha)
                {
                    _newVertexAlpha.Color.A = maximumAlpha;
                }
                else
                {
                    _newVertexAlpha.Color.A = (byte) (_newVertexAlpha.Color.A + alphaModifier);
                }

                vArray[_i] = _newVertexAlpha;
            }

            return vArray;
        }
    }
}