using Microsoft.Xna.Framework;

namespace BigBootyMod.Core.Physics.Particles
{
    /// <summary>
    /// Particles that adhere to gravity.
    /// </summary>
    public class GravityParticle : IdealParticle
    {
        public bool ObserveGravity;

        public GravityParticle(float x, float y, float mass, bool gravity) : base(x, y, mass) => ObserveGravity = gravity;

        public GravityParticle(Vector2 position, float mass, bool gravity) : base(position, mass) => ObserveGravity = gravity;

        public override void Update(float discreteTime)
        {
            if (ObserveGravity)
            {
                ApplyForce(new Vector2(0, Mass * PhysicsConstants.Gravity));
            }

            base.Update(discreteTime);
        }
    }
}