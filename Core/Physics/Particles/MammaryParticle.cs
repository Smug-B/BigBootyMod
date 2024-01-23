using Microsoft.Xna.Framework;

namespace BigBootyMod.Core.Physics.Particles
{
    public class MammaryParticle : ShapeMatchingParticles
    {
        public Vector2 Inertia = Vector2.One; // new Vector2(0.66f, 0.5f);

        public MammaryParticle(Vector2 position, float mass, Vector2 matchingForce) : base(position, mass, matchingForce)
        {
        }

        public MammaryParticle(float x, float y, float mass, Vector2 matchingForce) : base(x, y, mass, matchingForce)
        {
        }

        public override Vector2 CalculateAppliedForce(Vector2 force)
        {
            return force *= Inertia;
        }

        public override void Update(float discreteTime)
        {
            ApplyForce(new Vector2(0, Mass * PhysicsConstants.Gravity));
            base.Update(discreteTime);
            Force = Vector2.Zero;
        }
    }
}
