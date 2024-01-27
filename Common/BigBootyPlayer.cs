using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.DataStructures;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Diagnostics.CodeAnalysis;
using BigBootyMod.Core.Physics.Particles;
using BigBootyMod.Common.Physics;
using BigBootyMod.Core.Utils;
using BigBootyMod.Core.Physics;
using BigBootyMod.Common.Extensions;

namespace BigBootyMod.Common
{
    public class BigBootyPlayer : ModPlayer
    {
        public const int SamplingFactor = 4;

        public static int VertexCount => RenderPoints.Count;

        public static bool InWorld => Main.menuMode == 10;

        public GraphicsDevice GraphicsDevice => Main.graphics.GraphicsDevice;

        public static IList<BigBootyParticle> Points { get; private set; } = new List<BigBootyParticle>();

        public static IList<BigBootyLengthConstraints> Constraints { get; private set; } = new List<BigBootyLengthConstraints>();

        public static IList<BigBootyParticle> RenderPoints { get; private set; } = new List<BigBootyParticle>();

        [NotNull]
        public static VertexBuffer? VertexBuffer { get; private set; }

        [NotNull]
        public static IndexBuffer? Indicies { get; private set; }

        [NotNull]
        public static BasicEffect? RenderEffect { get; private set; }

        [NotNull]
        public static FieldInfo? SpriteBuffer { get; private set; }

        [NotNull]
        public static FieldInfo? Transform { get; private set; }

        public Vector2 CheeksAppliedForce { get; set; }

        public int JiggleTimer { get; private set; }

        public DrawData LegData;

        public void GenerateBigBootyData()
        {
            Texture2D bigBootyData = BigBootyMod.Request<Texture2D>("Assets/BigBootyData", AssetRequestMode.ImmediateLoad).Value;
            Color[] colorData = new Color[bigBootyData.Width * bigBootyData.Height];
            bigBootyData.GetData(colorData);

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

                    Vector2 position = new Vector2(i, j) + new Vector2(-16, -9) * SamplingFactor;
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

            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColorTexture), VertexCount, BufferUsage.WriteOnly);
            Indicies = new IndexBuffer(GraphicsDevice, typeof(int), VertexCount, BufferUsage.WriteOnly);
            Indicies.SetData(Enumerable.Range(0, VertexCount).ToArray());

            RenderEffect = new BasicEffect(GraphicsDevice);
            RenderEffect.TextureEnabled = true;
            RenderEffect.VertexColorEnabled = true;
        }

        public override void Load()
        {
            Main.QueueMainThreadAction(GenerateBigBootyData);

            On_PlayerDrawLayers.DrawPlayer_13_Leggings += FindLeggingData;
            On_PlayerDrawLayers.DrawPlayer_RenderAllLayers += RenderBigBooty;
        }

        public override void Unload()
        {
            Points.Clear();
            Constraints.Clear();
            RenderPoints.Clear();
            Main.QueueMainThreadAction(() =>
            {
                VertexBuffer?.Dispose();
                Indicies?.Dispose();
                RenderEffect?.Dispose();
            });
        }

        private void FindLeggingData(On_PlayerDrawLayers.orig_DrawPlayer_13_Leggings orig, ref PlayerDrawSet drawinfo)
        {
            int legIndex = drawinfo.DrawDataCache.Count;
            orig.Invoke(ref drawinfo);
            LegData = drawinfo.DrawDataCache.Count == legIndex ? default : drawinfo.DrawDataCache[legIndex];
        }

        private void RenderBigBooty(On_PlayerDrawLayers.orig_DrawPlayer_RenderAllLayers orig, ref PlayerDrawSet drawinfo)
        {
            List<DrawData> drawDataCache = drawinfo.DrawDataCache;
            DisposableSpriteDrawBuffer spriteBuffer = new DisposableSpriteDrawBuffer(Main.graphics.GraphicsDevice, 200);

            int end = drawDataCache.IndexOf(LegData);
            if (end == -1)
            {
                end = drawDataCache.Count;
            }
            else if (LegData.sourceRect.Value.Top / 56 != 5) // Not jumping
            {
                end++;
            }

            body(0, end, ref drawinfo);
            if (end != -1)
            {
                RasterizerState oldState = GraphicsDevice.RasterizerState;

                Texture2D legTexture = LegData.texture;
                GraphicsDevice.Indices = Indicies;
                GraphicsDevice.Textures[0] = legTexture;
                if (InWorld)
                {
                    GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                }

                bool drawCheekData(ref PlayerDrawSet drawinfo, bool leftCheek)
                {
                    VertexPositionColorTexture[]? cheekData = DrawBigBooty(ref drawinfo, leftCheek);
                    if (cheekData == null)
                    {
                        return false;
                    }

                    VertexBuffer.SetData(cheekData);
                    GraphicsDevice.SetVertexBuffer(VertexBuffer);
                    PlayerDrawHelper.SetShaderForData(drawinfo.drawPlayer, drawinfo.cHead, ref LegData);
                    foreach (var effectTechnique in RenderEffect.CurrentTechnique.Passes)
                    {
                        effectTechnique.Apply();
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Indicies.IndexCount, 0, Indicies.IndexCount);
                    }
                    return true;
                }

                bool hasDrawnAnything = drawCheekData(ref drawinfo, true);
                hasDrawnAnything = drawCheekData(ref drawinfo, false) || hasDrawnAnything; // Strong reliance on JIT just... not optimizing this away.

                if (hasDrawnAnything)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, InWorld ? Main.GameViewMatrix.TransformationMatrix : Main.UIScaleMatrix);
                }
                GraphicsDevice.RasterizerState = oldState;
            }

            if (end != drawDataCache.Count)
            {
                body(end, drawDataCache.Count, ref drawinfo);
            }

            spriteBuffer.Dispose();

            void body(int loopFrom, int loopTo, ref PlayerDrawSet drawinfo)
            {
                for (int i = loopFrom; i < loopTo; i++)
                {
                    DrawData drawData = drawDataCache[i];
                    if (drawData.texture != null)
                    {
                        drawData.Draw(spriteBuffer);
                    }
                }

                spriteBuffer.UploadAndBind();
                DrawData cdd = default;
                int num = 0;
                for (int i = loopFrom; i <= drawDataCache.Count; i++)
                {
                    if (drawinfo.projectileDrawPosition == i)
                    {
                        if (cdd.shader != 0)
                        {
                            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                        }
                        spriteBuffer.Unbind();
                        Projectile proj = Main.projectile[drawinfo.drawPlayer.heldProj];
                        if (!ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[proj.type])
                        {
                            proj.gfxOffY = drawinfo.drawPlayer.gfxOffY;
                        }
                        try
                        {
                            Main.instance.DrawProjDirect(proj);
                        }
                        catch
                        {
                            proj.active = false;
                        }
                        spriteBuffer.Bind();
                    }
                    if (i != drawDataCache.Count)
                    {
                        cdd = drawDataCache[i];
                        if (!cdd.sourceRect.HasValue)
                        {
                            cdd.sourceRect = cdd.texture.Frame();
                        }
                        PlayerDrawHelper.SetShaderForData(drawinfo.drawPlayer, drawinfo.cHead, ref cdd);
                        if (cdd.texture != null)
                        {
                            spriteBuffer.DrawSingle(num++);
                        }
                    }
                }
                spriteBuffer.Unbind();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            }
        }

        public override void PostUpdate()
        {
            if (Player.itemAnimation > 0 && ++JiggleTimer % 5 == 0 && Main.MouseWorld != Player.Center)
            {
                CheeksAppliedForce = new Vector2(0, Vector2.Normalize(Main.MouseWorld - Player.Center).Y * -18000);
            }

            CheeksAppliedForce *= 0.75f;

            int frame = Player.legFrame.Y / 56;
            foreach (BigBootyParticle verlet in Points)
            {
                verlet.ApplyForce(CheeksAppliedForce);
                verlet.LeftOriginalOffset = GetDrawOffset(frame, true) * SamplingFactor;
                verlet.RightOriginalOffset = GetDrawOffset(frame, false) * SamplingFactor;
                verlet.Update(PhysicsConstants.DeltaTime);
            }

            foreach (BigBootyLengthConstraints constraint in Constraints)
            {
                constraint.ApplyConstraint();
            }
        }

        public VertexPositionColorTexture[]? DrawBigBooty(ref PlayerDrawSet drawinfo, bool leftCheek)
        {
            int frame = drawinfo.drawPlayer.legFrame.Y / 56;
            if (LegData.Equals(default) ||
                (leftCheek && frame <= 12) ||
                drawinfo.drawPlayer.invis ||
                drawinfo.drawPlayer.mount.Active ||
                drawinfo.isSitting ||
                drawinfo.drawPlayer.legs == 140 ||
                drawinfo.shadow != 0 ||
                drawinfo.drawPlayer.legs == 169)
            {
                return null;
            }

            IList<VertexPositionColorTexture> output = new List<VertexPositionColorTexture>();
            int direction = LegData.effect == SpriteEffects.FlipHorizontally ? -1 : 1;
            return RenderPoints.Select(particle =>
            {
                Vector2 screenCoordinates;
                if (InWorld)
                {
                    Vector2 position = leftCheek ? particle.LeftPosition : particle.RightPosition;
                    if (direction == -1)
                    {
                        position.X *= -1;
                    }
                    screenCoordinates = position / SamplingFactor + LegData.position;
                    screenCoordinates = Vector2.Transform(screenCoordinates, Main.GameViewMatrix.ZoomMatrix);
                }
                else
                {
                    Vector2 position = leftCheek ? particle.LeftOriginalPosition : particle.RightOriginalPosition;
                    Vector2 offset = GetDrawOffset(frame, leftCheek);
                    screenCoordinates = position / SamplingFactor + LegData.position + offset;
                }
                Vector2 normalizedDeviceCoordinates = GraphicsUtils.ScreenToNormalizedDeviceCoordinates(screenCoordinates);
                return new VertexPositionColorTexture(new Vector3(normalizedDeviceCoordinates, 0), LegData.color, particle.TextureUVCoordinates);
            }).ToArray();
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
    }
}
