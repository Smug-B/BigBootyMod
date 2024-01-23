using Microsoft.Xna.Framework;

namespace BigBootyMod.Core.Physics.Particles
{
    /// <summary>
    /// Particles that aren't affected by physics in any way.
    /// These are good anchor points for other <see cref="IdealParticle"/> to latch onto.
    /// </summary>
    public class InertParticle : IdealParticle
    {
        public InertParticle(float x, float y) : base(x, y, -1) { }

        public InertParticle(Vector2 position) : base(position, -1) { }

        public override Vector2 CalculateAppliedForce(Vector2 force) => Vector2.Zero;

        public override void Update(float discreteTime) { }
    }
}
