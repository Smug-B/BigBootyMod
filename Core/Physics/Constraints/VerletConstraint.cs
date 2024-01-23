using BigBootyMod.Core.Physics.Particles;

namespace BigBootyMod.Core.Physics.Constraints
{
    public abstract class VerletConstraint
    {
        public IdealParticle FirstPoint;

        public IdealParticle SecondPoint;

        public VerletConstraint(IdealParticle firstPoint, IdealParticle secondPoint)
        {
            FirstPoint = firstPoint;
            SecondPoint = secondPoint;
        }

        public abstract void ApplyConstraint();
    }
}