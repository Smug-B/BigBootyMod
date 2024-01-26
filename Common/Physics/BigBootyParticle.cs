using BigBootyMod.Core.Utils;
using Microsoft.Xna.Framework;
using Terraria;

namespace BigBootyMod.Core.Physics.Particles
{
    /// <summary>
    /// Wacky particle that does not adhere to <see cref="IdealParticle"/>'s design at all.
    /// </summary>
    public class BigBootyParticle : IdealParticle
    {
        public const int SpriteSheetWidth = 40;

        public const int SpriteSheetHeight = 1120;

        public Vector2 TextureUVCoordinates { get; }

        public Vector2 LeftPosition { get; set; }

        public Vector2 LeftOldPosition { get; private set; }

        public Vector2 LeftOriginalPosition { get; }

        public Vector2 LeftOriginalOffset { get; set; }

        public Vector2 LeftForce { get; private set; }

        public Vector2 RightPosition { get => Position; set => Position = value; }

        public Vector2 RightOldPosition { get => OldPosition; private set => OldPosition = value; }

        public Vector2 RightOriginalPosition { get; }

        public Vector2 RightOriginalOffset { get; set; }

        public Vector2 RightForce { get => Force; private set => Force = value; }

        public Vector2 MatchForce { get; }

        public BigBootyParticle(Vector2 leftPos, Vector2 rightPos, float mass, Vector2 matchingForce, Vector2 textureCoordinates) : base(rightPos, mass)
        {
            LeftPosition = leftPos;
            LeftOldPosition = leftPos;
            LeftOriginalPosition = leftPos;

            RightOriginalPosition = rightPos;
            MatchForce = matchingForce;

            TextureUVCoordinates = GraphicsUtils.GetUVCoordinates(textureCoordinates, SpriteSheetWidth, SpriteSheetHeight);
        }

        public override Vector2 CalculateAppliedForce(Vector2 force) => Mass > 0 ? force : Vector2.Zero;

        public new void ApplyForce(Vector2 force)
        {
            LeftForce += CalculateAppliedForce(force);
            RightForce += CalculateAppliedForce(force);
        }

        public void ApplyForce(Vector2 leftForce, Vector2 rightForce)
        {
            LeftForce += CalculateAppliedForce(leftForce);
            RightForce += CalculateAppliedForce(rightForce);
        }

        public override void Update(float discreteTime)
        {
            Vector2 leftOriginal = LeftOriginalPosition + LeftOriginalOffset;
            if (LeftPosition != leftOriginal)
            {
                Vector2 correctionForce = LeftPosition.DirectionTo(leftOriginal) * LeftPosition.Distance(leftOriginal) * MatchForce;
                ApplyForce(correctionForce, Vector2.Zero);
            }

            Vector2 rightOriginal = RightOriginalPosition + RightOriginalOffset;
            if (RightPosition != rightOriginal)
            {
                Vector2 correctionForce = RightPosition.DirectionTo(rightOriginal) * RightPosition.Distance(rightOriginal) * MatchForce;
                ApplyForce(Vector2.Zero, correctionForce);
            }

            float discreteTimeSquared = discreteTime * discreteTime;
            Vector2 leftAcceleration = LeftForce / Mass;
            Vector2 newLeftPosition = 2 * LeftPosition - LeftOldPosition + leftAcceleration * discreteTimeSquared;
            LeftOldPosition = Vector2.Lerp(LeftPosition, newLeftPosition, 0.5f);
            LeftPosition = newLeftPosition;

            Vector2 rightAcceleration = RightForce / Mass;
            Vector2 newRightPosition = 2 * RightPosition - RightOldPosition + rightAcceleration * discreteTimeSquared;
            RightOldPosition = Vector2.Lerp(RightPosition, newRightPosition, 0.5f);
            RightPosition = newRightPosition;

            LeftForce = Vector2.Zero;
            RightForce = Vector2.Zero;
        }

        public float LeftDistance(BigBootyParticle otherPoint) => Vector2.Distance(LeftPosition, otherPoint.LeftPosition);

        public float RightDistance(BigBootyParticle otherPoint) => Vector2.Distance(RightPosition, otherPoint.RightPosition);
    }
}