using BigBootyMod.Core.Physics.Particles;
using Microsoft.Xna.Framework;

namespace BigBootyMod.Common.Physics
{
    public class BigBootyLengthConstraints 
    {
        public BigBootyParticle FirstPoint;

        public BigBootyParticle SecondPoint;

        public float Length;

        public BigBootyLengthConstraints(BigBootyParticle firstPoint, BigBootyParticle secondPoint, float? maximumLengthMultiplier = null)
        {
            FirstPoint = firstPoint;
            SecondPoint = secondPoint;
            Length = firstPoint.Distance(secondPoint) * maximumLengthMultiplier ?? 1f;
        }

        public void ApplyConstraint()
        {
            float leftDistance = FirstPoint.LeftDistance(SecondPoint);
            if (float.IsNormal(leftDistance) && leftDistance > Length)
            {
                float diffFactor = (Length - leftDistance) / leftDistance * 0.5f;
                Vector2 offset = (FirstPoint.LeftPosition - SecondPoint.LeftPosition) * diffFactor;
                if (FirstPoint.Mass > 0.0f)
                {
                    FirstPoint.LeftPosition += offset;
                }

                if (SecondPoint.Mass > 0.0f)
                {
                    SecondPoint.LeftPosition -= offset;
                }
            }

            float rightPosition = FirstPoint.RightDistance(SecondPoint);
            if (float.IsNormal(rightPosition) && rightPosition > Length)
            {
                float diffFactor = (Length - rightPosition) / rightPosition * 0.5f;
                Vector2 offset = (FirstPoint.RightPosition - SecondPoint.RightPosition) * diffFactor;
                if (FirstPoint.Mass > 0.0f)
                {
                    FirstPoint.RightPosition += offset;
                }

                if (SecondPoint.Mass > 0.0f)
                {
                    SecondPoint.RightPosition -= offset;
                }
            }
        }
    }
}
