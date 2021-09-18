using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Physics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
    internal class VitricBossSwoosh
    {
        readonly VitricBoss parent;
        readonly VerletChainInstance chain;
        readonly Effect fireEffect;
        Vector2 position;

        public VitricBossSwoosh(Vector2 offset, int length, VitricBoss parent)
        {
            position = offset;
            this.parent = parent;

            if (!Main.dedServ)
            {
                fireEffect = Filters.Scene["FireShader"].GetShader().Shader;

                chain = new VerletChainInstance(length, true, parent.npc.Center + position, 8)
                {
                    constraintRepetitions = 2,
                    drag = 1.5f,
                    forceGravity = new Vector2(0f, 0.25f),
                };
            }
        }

        public void ShiftPosition(Vector2 shift) => position += shift;

        public void Draw(SpriteBatch spriteBatch)
        {
            var matrix = new Matrix
            (
                Main.GameViewMatrix.Zoom.X, 0, 0, 0,
                0, Main.GameViewMatrix.Zoom.X, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );

            fireEffect.Parameters["time"].SetValue(-Main.GameUpdateCount / 45f);
            fireEffect.Parameters["upscale"].SetValue(matrix);
            fireEffect.Parameters["sampleTexture"].SetValue(GetTexture(AssetDirectory.Assets + "FireTrail"));

            chain.DrawStrip(PrepareStrip, fireEffect);
            chain.UpdateChain(parent.npc.Center + parent.PainOffset + Vector2.UnitX * -parent.twistTarget * 18 + position.RotatedBy(parent.npc.rotation));
            chain.IterateRope(WindForce);
        }

        public VertexBuffer PrepareStrip(Vector2 offset)
        {
            var buff = new VertexBuffer(Main.graphics.GraphicsDevice, typeof(VertexPositionColorTexture), chain.segmentCount * 9 - 6, BufferUsage.WriteOnly);

            VertexPositionColorTexture[] verticies = new VertexPositionColorTexture[chain.segmentCount * 9 - 6];

            float rotation = (chain.ropeSegments[0].posScreen - chain.ropeSegments[1].posScreen).ToRotation() + (float)Math.PI / 2;

            verticies[0] = new VertexPositionColorTexture((chain.ropeSegments[0].posScreen + offset + Vector2.UnitY.RotatedBy(rotation - Math.PI / 4) * -5).Vec3().ScreenCoord(), chain.ropeSegments[0].color, new Vector2(0, 0.2f));
            verticies[1] = new VertexPositionColorTexture((chain.ropeSegments[0].posScreen + offset + Vector2.UnitY.RotatedBy(rotation + Math.PI / 4) * -5).Vec3().ScreenCoord(), chain.ropeSegments[0].color, new Vector2(0, 0.8f));
            verticies[2] = new VertexPositionColorTexture((chain.ropeSegments[1].posScreen + offset).Vec3().ScreenCoord(), chain.ropeSegments[1].color, new Vector2(0, 0.5f));

            for (int k = 1; k < chain.segmentCount - 1; k++)
            {
                float progress = k / 3f;
                float rotation2 = (chain.ropeSegments[k - 1].posScreen - chain.ropeSegments[k].posScreen).ToRotation() + (float)Math.PI / 2;
                float scale = 2.4f;

                int point = k * 9 - 6;

                verticies[point] = new VertexPositionColorTexture((chain.ropeSegments[k].posScreen + offset + Vector2.UnitY.RotatedBy(rotation2 - Math.PI / 4) * -(chain.segmentCount - k) * scale).Vec3().ScreenCoord(), chain.ropeSegments[k].color, new Vector2(progress, 0.2f));
                verticies[point + 1] = new VertexPositionColorTexture((chain.ropeSegments[k].posScreen + offset + Vector2.UnitY.RotatedBy(rotation2 + Math.PI / 4) * -(chain.segmentCount - k) * scale).Vec3().ScreenCoord(), chain.ropeSegments[k].color, new Vector2(progress, 0.8f));
                verticies[point + 2] = new VertexPositionColorTexture((chain.ropeSegments[k + 1].posScreen + offset).Vec3().ScreenCoord(), chain.ropeSegments[k + 1].color, new Vector2(progress + 1/3f, 0.5f));

                int extra = k == 1 ? 0 : 6;
                verticies[point + 3] = verticies[point];
                verticies[point + 4] = verticies[point - (3 + extra)];
                verticies[point + 5] = verticies[point - (1 + extra)];

                verticies[point + 6] = verticies[point - (2 + extra)];
                verticies[point + 7] = verticies[point + 1];
                verticies[point + 8] = verticies[point - (1 + extra)];
            }

            buff.SetData(verticies);

            return buff;
        }

        public void DrawAdditive(SpriteBatch sb)
        {
            var tex = GetTexture(AssetDirectory.Assets + "Keys/GlowSoft");

            for (int k = 0; k < chain.segmentCount; k++)
            {
                var segment = chain.ropeSegments[k];
                var progress = 1.35f - (float)k / chain.segmentCount;
                var progress2 = 1.22f - (float)k / chain.segmentCount;
                sb.Draw(tex, segment.posNow - Main.screenPosition, null, segment.color * progress * 0.80f, 0, tex.Size() / 2, progress2, 0, 0);
            }
        }

        private void WindForce(int index)//wind
        {
            float opacity = 0;

            if (parent.Phase >= (int)VitricBoss.AIStates.FirstToSecond)
            {
                opacity = 1;

                if (parent.Phase == (int)VitricBoss.AIStates.FirstToSecond)
                    opacity = MathHelper.Clamp((parent.GlobalTimer - 360) / 60f, 0, 1);
            }

            float sin = (float)Math.Sin(StarlightWorld.rottime + index);

            Vector2 pos = Vector2.UnitX.RotatedBy(position.ToRotation()) * 2 + Vector2.UnitY.RotatedBy(position.ToRotation()) * sin * 2.1f;

            Color color = new Color(230 - (int)(20 * sin), 120 + (int)(30 * sin), 55);

            if(index < 5)
            color *= 1 + (opacity * (1 - index / 5f)) * 2;

            chain.ropeSegments[index].posNow += pos;
            chain.ropeSegments[index].color = color;
        }
    }
}