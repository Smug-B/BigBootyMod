using BigBootyMod.Core.Physics.Constraints;
using BigBootyMod.Core.Physics.Particles;
using BigBootyMod.Core.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Graphics.Renderers;

namespace BigBootyMod.Common
{
    public class Cheek
    {
        public static IList<BigBootyParticle> Points { get; private set; } = new List<BigBootyParticle>();

        public static IList<VerletConstraint> Constraints { get; private set; } = new List<VerletConstraint>();

        public static IList<BigBootyParticle> RenderPoints { get; private set; } = new List<BigBootyParticle>();

        public static int VertexCount => RenderPoints.Count;

        public bool LeftCheek { get; set; }

        public static void GenerateStaticBootyData(Texture2D bigBootyData, Color[] colorData)
        {
            IDictionary<Point, BigBootyParticle> pointData = new Dictionary<Point, BigBootyParticle>();
            for (int i = 0; i < bigBootyData.Width; i++)
            {
                for (int j = 0; j < bigBootyData.Height; j++)
                {
                    Color color = colorData[i * bigBootyData.Width + j];
                    if (color.A == 0)
                    {
                        continue;
                    }

                    Vector2 textureSampleCoordinates = Vector2.Zero;
                    if (color.R == 255)
                    {
                        textureSampleCoordinates = new Vector2(21, 43); // 21, 43
                    }

                    if (color.R == 150)
                    {
                        textureSampleCoordinates = new Vector2(19, 45);
                    }

                    if (color.R == 100)
                    {
                        textureSampleCoordinates = new Vector2(17, 45);
                    }

                    if (color.R == 0)
                    {
                        textureSampleCoordinates = new Vector2(27, 43);
                    }

                    pointData.Add(new Point(i, j), new BigBootyParticle(i, j, 1, new Vector2(5), textureSampleCoordinates));
                }
            }

            Points = pointData.Values.ToList();

            for (int i = 0; i < bigBootyData.Width; i++)
            {
                for (int j = 0; j < bigBootyData.Height; j++)
                {
                    if (!pointData.HasSafeValue(new Point(i, j)))
                    {
                        continue;
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

        public Vector2 GetDrawOffset(int frame)
        {
            Vector2 offset = new Vector2(-22, -39);
            if (frame <= 12)
            {
                return frame switch
                {
                    < 5 or 10 or 11 => new Vector2(8, 30),
                    5 => new Vector2(6, 30),
                    6 or 12 => new Vector2(10, 30),
                    < 10 => new Vector2(8, 28),
                    _ => Vector2.Zero
                } + offset;
            }

            if (LeftCheek)
            {
                return frame switch
                {
                    >= 13 and <= 16 or 19 => new Vector2(8, 30),
                    17 => new Vector2(6, 32),
                    18 => new Vector2(8, 32),
                    _ => Vector2.Zero
                } + offset;
            }

            return frame switch
            {
                13 or 14 or 16 or 19 => new Vector2(12, 30),
                15 => new Vector2(12, 28),
                17 => new Vector2(10, 32),
                18 => new Vector2(12, 32),
                _ => Vector2.Zero
            } + offset;
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
            foreach (BigBootyParticle particle in RenderPoints)
            {
                Vector2 screenCoordinates = particle.Position / 2f + leggingDrawData.position + GetDrawOffset(frame);
                Vector2 normalizedDeviceCoordinates = GraphicsUtils.ScreenToNormalizedDeviceCoordinates(screenCoordinates);
                output.Add(new VertexPositionColorTexture(new Vector3(normalizedDeviceCoordinates, 0), Color.White, particle.TextureUVCoordinates));
            }
            return output.ToArray();
        }
    }
}
