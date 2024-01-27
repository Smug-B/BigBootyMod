using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BigBootyMod.Common
{
    public struct BigBootyParticle
    {
        public const int SpriteSheetWidth = 40;

        public const int SpriteSheetHeight = 1120;

        public Vector2 Position;

        public Vector2 TextureUVCoordinates;

        public BigBootyParticle(Vector2 position, Vector2 textureCoordinates, float samplingFactor = 4f)
        {
            Position = position / samplingFactor;
            TextureUVCoordinates = new Vector2(textureCoordinates.X / SpriteSheetWidth, textureCoordinates.Y / SpriteSheetHeight);
        }

        public VertexPositionColorTexture ToVertex(Vector2 normalizedPosition, Color color)
        {
            normalizedPosition.Y = 1 - normalizedPosition.Y;
            return new VertexPositionColorTexture(new Vector3(normalizedPosition * 2 - Vector2.One, 0), color, TextureUVCoordinates);
        }
    }
}