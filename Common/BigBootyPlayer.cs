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

namespace BigBootyMod.Common
{
    public class BigBootyPlayer : ModPlayer
    {
        public GraphicsDevice GraphicsDevice => Main.graphics.GraphicsDevice;

        public Cheek LeftCheek { get; } = new Cheek();

        public Cheek RightCheek { get; } = new Cheek();

        public HashSet<DrawData> LegData = new HashSet<DrawData>();

        public DrawData LegDataData;

        [NotNull]
        public VertexBuffer? VertexBuffer { get; private set; }

        [NotNull]
        public IndexBuffer? Indicies { get; private set; }

        [NotNull]
        public BasicEffect? RenderEffect { get; private set; }

        [NotNull]
        public FieldInfo? SpriteBuffer { get; private set; }

        [NotNull]
        public FieldInfo? Transform { get; private set; }

        [MaybeNull]
        public Matrix? Matrix { get; private set; }

        public void GenerateBigBootyData()
        {
            Texture2D bigBootyData = BigBootyMod.Request<Texture2D>("Assets/BigBootyData", AssetRequestMode.ImmediateLoad).Value;
            Color[] colorData = new Color[bigBootyData.Width * bigBootyData.Height];
            bigBootyData.GetData(colorData);

            Cheek.GenerateStaticBootyData(bigBootyData, colorData);
            LeftCheek.LeftCheek = true;

            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColorTexture), Cheek.VertexCount, BufferUsage.WriteOnly);
            Indicies = new IndexBuffer(GraphicsDevice, typeof(int), Cheek.VertexCount, BufferUsage.WriteOnly);
            Indicies.SetData(Enumerable.Range(0, Cheek.VertexCount).ToArray());

            RenderEffect = new BasicEffect(GraphicsDevice);
            RenderEffect.TextureEnabled = true;
            RenderEffect.VertexColorEnabled = true;
        }

        public override void Load()
        {
            Main.QueueMainThreadAction(GenerateBigBootyData);

            On_PlayerDrawLayers.DrawPlayer_13_Leggings += FindLeggingData;
            On_PlayerDrawLayers.DrawPlayer_RenderAllLayers += RenderBigBooty;

            SpriteBuffer = typeof(PlayerDrawLayers).GetField("spriteBuffer", BindingFlags.NonPublic | BindingFlags.Static);
            Transform = typeof(SpriteBatch).GetField("transformMatrix", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private void FindLeggingData(On_PlayerDrawLayers.orig_DrawPlayer_13_Leggings orig, ref PlayerDrawSet drawinfo)
        {
            HashSet<DrawData> preDraw = new HashSet<DrawData>(drawinfo.DrawDataCache);
            orig.Invoke(ref drawinfo);
            LegData = new HashSet<DrawData>(drawinfo.DrawDataCache);
            LegData.RemoveWhere(preDraw.Contains);
            LegDataData = LegData.FirstOrDefault();
        }

        private void RenderBigBooty(On_PlayerDrawLayers.orig_DrawPlayer_RenderAllLayers orig, ref PlayerDrawSet drawinfo)
        {
            List<DrawData> drawDataCache = drawinfo.DrawDataCache;
            SpriteDrawBuffer? spriteBuffer = (SpriteDrawBuffer?)SpriteBuffer.GetValue(null);
            if (spriteBuffer == null)
            {
                spriteBuffer = new SpriteDrawBuffer(Main.graphics.GraphicsDevice, 200);
            }
            else
            {
                spriteBuffer.CheckGraphicsDevice(Main.graphics.GraphicsDevice);
            }

            int end = drawDataCache.IndexOf(LegDataData);
            if (end == -1)
            {
                end = drawDataCache.Count;
            }
            else
            {
                end++;
            }

            body(0, end, ref drawinfo);
            if (end != -1)
            {
                Matrix = (Matrix)Transform.GetValue(Main.spriteBatch);

                Texture2D legTexture = LegDataData.texture;
                GraphicsDevice.Indices = Indicies;
                GraphicsDevice.Textures[0] = legTexture;

                VertexPositionColorTexture[]? leftCheek = LeftCheek.DrawBigBooty(legTexture, LegDataData);
                if (leftCheek != null)
                {
                    VertexBuffer.SetData(leftCheek);
                    GraphicsDevice.SetVertexBuffer(VertexBuffer);
                    foreach (var effectTechnique in RenderEffect.CurrentTechnique.Passes)
                    {
                        effectTechnique.Apply();
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Indicies.IndexCount, 0, Indicies.IndexCount / 3);
                    }
                }

                VertexBuffer.SetData(RightCheek.DrawBigBooty(legTexture, LegDataData));
                GraphicsDevice.SetVertexBuffer(VertexBuffer);
                foreach (var effectTechnique in RenderEffect.CurrentTechnique.Passes)
                {
                    effectTechnique.Apply();
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Indicies.IndexCount, 0, Indicies.IndexCount / 3);
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Matrix.Value);
            } // Showed Code

            if (end != drawDataCache.Count)
            {
                body(end, drawDataCache.Count, ref drawinfo);
            }

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
    }
}
