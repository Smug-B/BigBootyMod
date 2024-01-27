using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.DataStructures;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Diagnostics.CodeAnalysis;
using BigBootyMod.Common.Extensions;

namespace BigBootyMod.Common
{
    public class BigBootySystem : ModSystem
    {
        public const int SamplingFactor = 4;

        public static int VertexCount => RenderPoints.Count;

        public static bool InWorld => Main.menuMode == 10;

        public GraphicsDevice GraphicsDevice => Main.graphics.GraphicsDevice;

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

        public static bool MultiSample { get; internal set; }

        private DrawData LegData;

        public void GenerateBigBootyData()
        {
            Texture2D bigBootyData = BigBootyMod.Request<Texture2D>("Assets/BigBootyData", AssetRequestMode.ImmediateLoad).Value;

            Color[] colorData = new Color[bigBootyData.Width * bigBootyData.Height];
            bigBootyData.GetData(colorData);

            Dictionary<Point, BigBootyParticle> pointData = new Dictionary<Point, BigBootyParticle>();
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

                    Vector2 position = new Vector2(i, j) + new Vector2(-16, -9) * SamplingFactor;
                    pointData.Add(new Point(i, j), new BigBootyParticle(position, textureSampleCoordinates));
                }
            }

            for (int i = 0; i < bigBootyData.Width; i++)
            {
                for (int j = 0; j < bigBootyData.Height; j++)
                {
                    if (!pointData.ContainsKey(new Point(i, j - 1)))
                    {
                        continue;
                    }

                    if (pointData.ContainsKey(new Point(i + 1, j)) && pointData.ContainsKey(new Point(i, j + 1)))
                    {
                        RenderPoints.Add(pointData[new Point(i + 1, j)]);
                        RenderPoints.Add(pointData[new Point(i, j)]);
                        RenderPoints.Add(pointData[new Point(i, j + 1)]);
                    }

                    if (pointData.ContainsKey(new Point(i - 1, j)) && pointData.ContainsKey(new Point(i, j - 1)))
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
            else if (drawinfo.drawPlayer.legFrame.Y / 56 != 5) // Not jumping
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

        public VertexPositionColorTexture[]? DrawBigBooty(ref PlayerDrawSet drawinfo, bool leftCheek)
        {
            int frame = drawinfo.drawPlayer.legFrame.Y / 56;
            if (LegData.Equals(default) ||
                (leftCheek && frame <= 12) ||
                drawinfo.drawPlayer.invis ||
                drawinfo.drawPlayer.mount.Active ||
                drawinfo.isSitting ||
                drawinfo.drawPlayer.legs == 140 ||
                (!MultiSample && drawinfo.shadow != 0) ||
                drawinfo.drawPlayer.legs == 169)
            {
                return null;
            }

            Vector2 screenSize = Main.ScreenSize.ToVector2();
            Vector2 direction = new Vector2(LegData.effect == SpriteEffects.FlipHorizontally ? -1 : 1, 1);
            Vector2 drawOffset = GetDrawOffset(frame, leftCheek);

            /*VertexPositionColorTexture[] verticies = new VertexPositionColorTexture[VertexCount];
            for (int i = 0; i < verticies.Length; i++)
            {
                BigBootyParticle bigBootyParticle = RenderPoints[i];
                Vector2 normalizedCoordinates = Vector2.Transform((bigBootyParticle.Position + drawOffset) * direction + LegData.position, Main.GameViewMatrix.ZoomMatrix) 
                    / screenSize;
                normalizedCoordinates.Y = 1 - normalizedCoordinates.Y;
                verticies[i] = new VertexPositionColorTexture(new Vector3(normalizedCoordinates * 2 - Vector2.One, 0), LegData.color, bigBootyParticle.TextureUVCoordinates);
            }
            return verticies;*/

            // Unconclusive as to which one 'runs faster'. 
            return RenderPoints.Select(bigBootyParticle =>
            {
                // (bigBootyParticle.Position + drawOffset) can be pre-calculated.
                Vector2 normalizedCoordinates = (bigBootyParticle.Position + drawOffset) * direction + LegData.position;
                if (InWorld)
                {
                    normalizedCoordinates = Vector2.Transform(normalizedCoordinates, Main.GameViewMatrix.ZoomMatrix);
                }
                return bigBootyParticle.ToVertex(normalizedCoordinates / screenSize, LegData.color);
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
