using BigBootyMod.Common.Physics;
using BigBootyMod.Core.Physics;
using BigBootyMod.Core.Physics.Particles;
using BigBootyMod.Core.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace BigBootyMod.Common
{
    public class Cheek
    {
        public static IList<BigBootyParticle> Points { get; private set; } = new List<BigBootyParticle>();

        public static IList<BigBootyLengthConstraints> Constraints { get; private set; } = new List<BigBootyLengthConstraints>();

        public static IList<BigBootyParticle> RenderPoints { get; private set; } = new List<BigBootyParticle>();

        public static int SamplingFactor = 4;

        public static int VertexCount => RenderPoints.Count;

        public bool LeftCheek { get; }

        public Cheek(bool leftCheek) => LeftCheek = leftCheek;

        public static void GenerateStaticBootyData(Texture2D bigBootyData, Color[] colorData)
        {
            IDictionary<Point, BigBootyParticle> pointData = new Dictionary<Point, BigBootyParticle>();
            for (int j = 0; j < bigBootyData.Height; j++)
            {
                for (int i = 0; i < bigBootyData.Width; i++)
                {
                    Color color = colorData[j * bigBootyData.Width + i];
                    if (color.A == 0)
                    {
                        continue;
                    }

                    Vector2 textureSampleCoordinates = Vector2.Zero;
                    if (color.R == 255)
                    {
                        textureSampleCoordinates = new Vector2(21, 43);
                    }
                    else if (color.R == 150)
                    {
                        textureSampleCoordinates = new Vector2(19, 45);
                    }
                    else if (color.R == 100)
                    {
                        textureSampleCoordinates = new Vector2(17, 45);
                    }
                    else if (color.R == 0)
                    {
                        textureSampleCoordinates = new Vector2(27, 43);
                    }

                    Vector2 matchingForce = new Vector2(3000);
                    float mass = 1;
                    if (color.G == 255)
                    {
                        mass = -1;
                    }
                    else if (color.G == 100)
                    {
                        matchingForce *= 2;
                    }

                    Vector2 position = new Vector2(i, j) + new Vector2(-16, -9) * 4;
                    pointData.Add(new Point(i, j), new BigBootyParticle(position, position, mass, matchingForce, textureSampleCoordinates));
                }
            }

            Points = pointData.Values.ToList();

            for (int i = 0; i < bigBootyData.Width; i++)
            {
                for (int j = 0; j < bigBootyData.Height; j++)
                {
                    if (!pointData.TryGetValue(new Point(i, j - 1), out BigBootyParticle? value))
                    {
                        continue;
                    }

                    if (pointData.TryGetValue(new Point(i, j - 1), out BigBootyParticle? top))
                    {
                        Constraints.Add(new BigBootyLengthConstraints(value, top));
                    }

                    if (pointData.TryGetValue(new Point(i, j + 1), out BigBootyParticle? bottom))
                    {
                        Constraints.Add(new BigBootyLengthConstraints(value, bottom));
                    }

                    if (pointData.TryGetValue(new Point(i - 1, j), out BigBootyParticle? left))
                    {
                        Constraints.Add(new BigBootyLengthConstraints(value, left));
                    }

                    if (pointData.TryGetValue(new Point(i + 1, j), out BigBootyParticle? right))
                    {
                        Constraints.Add(new BigBootyLengthConstraints(value, right));
                    }


                    if (pointData.HasSafeValue(new Point(i + 1, j)) && pointData.HasSafeValue(new Point(i, j + 1)))
                    {
                        RenderPoints.Add(pointData[new Point(i + 1, j)]);
                        RenderPoints.Add(pointData[new Point(i, j)]);
                        RenderPoints.Add(pointData[new Point(i, j + 1)]);
                    }

                    if (pointData.HasSafeValue(new Point(i - 1, j)) && pointData.HasSafeValue(new Point(i, j - 1)))
                    {
                        RenderPoints.Add(pointData[new Point(i - 1, j)]);
                        RenderPoints.Add(pointData[new Point(i, j)]);
                        RenderPoints.Add(pointData[new Point(i, j - 1)]);
                    }
                }
            }
        }

        public static void Update(Player player)
        {
            int frame = player.legFrame.Y / 56;

            foreach (BigBootyParticle verlet in Points)
            {
                verlet.LeftOriginalOffset = GetDrawOffset(frame, true) * 4;
                verlet.RightOriginalOffset = GetDrawOffset(frame, false) * 4;
                verlet.Update(PhysicsConstants.DeltaTime);
            }

            foreach (BigBootyLengthConstraints constraint in Constraints)
            {
                constraint.ApplyConstraint();
            }
        }

        // Offset from the first frame
        public static Vector2 GetDrawOffset(int frame, bool left)
        {
            if (frame <= 12)
            {
                return frame switch
                {
                    5 => new Vector2(2, -1),
                    6 or 12 => new Vector2(2, 0),
                    <= 10 => new Vector2(2, -1),
                    _ => Vector2.Zero
                };
            }

            if (left)
            {
                return Vector2.Zero;
            }

            return frame switch
            {
                13 or 19 => new Vector2(4, 0),
                14 or 15 => new Vector2(4, -1),
                16 or 17 or 18 => new Vector2(6, 0),
                _ => Vector2.Zero
            };
        }

        public VertexPositionColorTexture[]? DrawBigBooty(Texture2D leggings, DrawData leggingDrawData)
        {
            if (RenderPoints == null)
            {
                throw new Exception("Tried to make cake without the necessary ingredients.");
            }

            int frame = leggingDrawData.sourceRect.Value.Top / 56;
            if (LeftCheek && frame <= 12)
            {
                return null;
            }

            IList<VertexPositionColorTexture> output = new List<VertexPositionColorTexture>();
            int direction = leggingDrawData.effect == SpriteEffects.FlipHorizontally ? -1 : 1;
            return RenderPoints.Select(particle =>
            {
                Vector2 screenCoordinates;
                if (BigBootyPlayer.InWorld)
                {
                    Vector2 position = LeftCheek ? particle.LeftPosition : particle.RightPosition;
                    if (direction == -1)
                    {
                        position.X *= -1;
                        //position.X += 4;
                    }
                    screenCoordinates = position / 4f + leggingDrawData.position;
                    screenCoordinates = Vector2.Transform(screenCoordinates, Main.GameViewMatrix.ZoomMatrix);
                }
                else
                {
                    Vector2 position = LeftCheek ? particle.LeftOriginalPosition : particle.RightOriginalPosition;
                    Vector2 offset = GetDrawOffset(frame, LeftCheek);
                    screenCoordinates = position / 4f + leggingDrawData.position + offset;
                }
                Vector2 normalizedDeviceCoordinates = GraphicsUtils.ScreenToNormalizedDeviceCoordinates(screenCoordinates);
                Color color = BigBootyPlayer.InWorld ? new Color(Lighting.GetSubLight(screenCoordinates + Main.screenPosition)) : Color.White;
                return new VertexPositionColorTexture(new Vector3(normalizedDeviceCoordinates, 0), color, particle.TextureUVCoordinates);
            }).ToArray();
        }
    }
}
