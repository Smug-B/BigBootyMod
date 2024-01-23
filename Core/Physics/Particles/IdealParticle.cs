using Microsoft.Xna.Framework;

namespace BigBootyMod.Core.Physics.Particles
{
    public class IdealParticle
    {
        public Vector2 Position;

        public Vector2 OldPosition;

        public float Mass;

        public Vector2 Force { get; protected set; }

        public IdealParticle(float x, float y, float mass) : this(new Vector2(x, y), mass) { }

        public IdealParticle(Vector2 position, float mass)
        {
            Position = position;
            OldPosition = position;
            Mass = mass;
        }

        public virtual void Update(float discreteTime)
        {
            Vector2 acceleration = Force / Mass;
            Vector2 newPosition = 2 * Position - OldPosition + acceleration * discreteTime * discreteTime;
            OldPosition = Position;
            Position = newPosition;
        }

        public virtual Vector2 CalculateAppliedForce(Vector2 force) => force;

        public void ApplyForce(Vector2 force) => Force += CalculateAppliedForce(force);

        public float Distance(IdealParticle otherPoint) => Vector2.Distance(Position, otherPoint.Position);

        public float DistanceSquared(IdealParticle otherPoint) => Vector2.DistanceSquared(Position, otherPoint.Position);
    }
}
