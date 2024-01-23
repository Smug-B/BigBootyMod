using BigBootyMod.Core.Physics.Particles;
using Microsoft.Xna.Framework;

namespace BigBootyMod.Core.Physics.Constraints
{
    public class MinLengthConstraint : VerletConstraint
    {
        public float Length;

        public MinLengthConstraint(IdealParticle firstPoint, IdealParticle secondPoint, float? minimumLengthMultiplier = null) : base(firstPoint, secondPoint)
        {
            Length = firstPoint.Distance(secondPoint) * minimumLengthMultiplier ?? 1f;
        }

        public override void ApplyConstraint()
        {
            float distance = FirstPoint.Distance(SecondPoint);
            if (!float.IsNormal(distance) || distance >= Length)
            {
                return;
            }

            float diffFactor = (Length - distance) / distance * 0.5f;
            Vector2 offset = (FirstPoint.Position - SecondPoint.Position) * diffFactor;
            if (FirstPoint.Mass > 0.0f)
            {
                FirstPoint.Position += offset;
            }

            if (SecondPoint.Mass > 0.0f)
            {
                SecondPoint.Position -= offset;
            }
        }
    }
}