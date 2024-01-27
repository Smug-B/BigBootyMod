using BigBootyMod.Core.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BigBootyMod.Common.Physics.Particles
{
    public struct BigBootyParticle
    {
        public const int SpriteSheetWidth = 40;

        public const int SpriteSheetHeight = 1120;

        public Vector2 Position { get; }

        public Vector2 TextureUVCoordinates { get; }

        public BigBootyParticle(Vector2 position, Vector2 textureCoordinates)
        {
            Position = position;
            TextureUVCoordinates = GraphicsUtils.GetUVCoordinates(textureCoordinates, SpriteSheetWidth, SpriteSheetHeight);
        }

        public VertexPositionColorTexture ToVertex(Vector2 position, Color color)
        {
            Vector2 screenCoordinates = GraphicsUtils.ScreenToNormalizedDeviceCoordinates(position);
            return new VertexPositionColorTexture(new Vector3(screenCoordinates, 0), color, TextureUVCoordinates);
        }
    }
}