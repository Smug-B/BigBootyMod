using Microsoft.Xna.Framework;
using Terraria;

namespace BigBootyMod.Core.Physics.Particles
{
    /// <summary>
    /// Particles that 'remember' their original positions and attempt to return to them.
    /// </summary>
    public class ShapeMatchingParticles : IdealParticle
    {
        public Vector2 OriginalPosition;

        public Vector2 MatchForce;

        public ShapeMatchingParticles(float x, float y, float mass, Vector2 matchingForce) : base(x, y, mass)
        {
            OriginalPosition = new Vector2(x, y);
            MatchForce = matchingForce;
        }

        public ShapeMatchingParticles(Vector2 position, float mass, Vector2 matchingForce) : base(position, mass)
        {
            OriginalPosition = position;
            MatchForce = matchingForce;
        }

        public override void Update(float discreteTime)
        {
            if (Position != OriginalPosition)
            {
                Vector2 correctionForce = Position.DirectionTo(OriginalPosition) * Position.Distance(OriginalPosition) * MatchForce;
                ApplyForce(correctionForce);
            }

            base.Update(discreteTime);
        }
    }
}
