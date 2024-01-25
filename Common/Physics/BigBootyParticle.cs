using BigBootyMod.Core.Utils;
using Microsoft.Xna.Framework;

namespace BigBootyMod.Core.Physics.Particles
{
    public class BigBootyParticle : ShapeMatchingParticles
    {
        public const int SpriteSheetWidth = 40;

        public const int SpriteSheetHeight = 1120;

        public Vector2 LeftOriginalPosition { get; }

        public Vector2 TextureCoordinates { get; }

        public Vector2 TextureUVCoordinates { get; }

        public Vector2 ActualOriginalPositions { get; }


        public BigBootyParticle(Vector2 position, float mass, Vector2 matchingForce, Vector2 textureCoordinates) : base(position, mass, matchingForce)
        {
            ActualOriginalPositions = position;
            TextureCoordinates = textureCoordinates;
            TextureUVCoordinates = GraphicsUtils.GetUVCoordinates(textureCoordinates, SpriteSheetWidth, SpriteSheetHeight);
        }

        public BigBootyParticle(float x, float y, float mass, Vector2 matchingForce, Vector2 textureCoordinates) : base(x, y, mass, matchingForce)
        {
            ActualOriginalPositions = new Vector2(x, y);
            TextureCoordinates = textureCoordinates;
            TextureUVCoordinates = GraphicsUtils.GetUVCoordinates(textureCoordinates, SpriteSheetWidth, SpriteSheetHeight);
        }

        public override void Update(float discreteTime)
        {
            ApplyForce(new Vector2(0, Mass * PhysicsConstants.Gravity));
            base.Update(discreteTime);
            Force = Vector2.Zero;
        }
    }
}