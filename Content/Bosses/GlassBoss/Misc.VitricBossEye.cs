using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Physics;
using StarlightRiver.Helpers;
using System;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    internal class VitricBossEye
    {
        private Vector2 Position;
        public VitricBoss Parent;
        private int Timer;
        private readonly int Index;

        public VitricBossEye(Vector2 pos, int index)
        {
            Position = pos;
            Index = index;
        }

        public void Draw(SpriteBatch sb)
        {
            if (Parent == null) 
                return;
            Texture2D tex = GetTexture(AssetDirectory.GlassBoss + "VitricBossEye");
            float rot = (Parent.npc.position + Position - Main.player[Parent.npc.target].Center).ToRotation();
            if (Parent.npc.target >= 200) 
                rot = 0;
            Color color = new Color(255, 210, 90);
            if ((Parent.npc.ai[0] > 360 || Timer >= 1) && Timer < 15) 
                Timer++;
            if (Parent.npc.ai[1] != (int)VitricBoss.AIStates.SpawnAnimation && Parent.npc.ai[0] % 120 == Index * 6) 
                Timer = 1;
            if (Parent.npc.ai[1] == (int)VitricBoss.AIStates.Anger)
            {
                rot = StarlightWorld.rottime * 4 + Index * 2;
                color = new Color(205, 120, 255);
                Timer = 15;
            }

            sb.Draw(tex, Parent.npc.position + Position + new Vector2(-1, 0).RotatedBy(rot) * 3 - Main.screenPosition, tex.Frame(), color, 0, tex.Size() / 2, Timer / 15f, 0, 0);
        }
    }

    internal class VitricBossSwoosh
    {
        VitricBoss parent;
        Vector2 position;
        VerletChainInstance chain;
        Effect fireEffect = Filters.Scene["FireShader"].GetShader().Shader;

        public VitricBossSwoosh(Vector2 offset, int length, VitricBoss parent)
        {
            position = offset;
            this.parent = parent;

            chain = new VerletChainInstance(true)
            {
                segmentCount = length,
                segmentDistance = 8,
                constraintRepetitions = 2,
                drag = 1.5f,
                forceGravity = new Vector2(0f, 0.25f),
                gravityStrengthMult = 1f
            };
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            fireEffect.Parameters["time"].SetValue(-Main.GameUpdateCount / 45f);
            fireEffect.Parameters["upscale"].SetValue(Main.GameViewMatrix.ZoomMatrix);
            fireEffect.Parameters["sampleTexture"].SetValue(GetTexture(AssetDirectory.Assets + "FireTrail"));

            chain.DrawStrip(PrepareStrip, fireEffect);
            chain.UpdateChain(parent.npc.Center + position);
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

            for (int k = 2; k < chain.segmentCount; k++)
            {
                var segment = chain.ropeSegments[k];
                var progress = 1.35f - (float)k / chain.segmentCount;
                var progress2 = 1.22f - (float)k / chain.segmentCount;
                sb.Draw(tex, segment.posNow - Main.screenPosition, null, segment.color * progress * 0.80f, 0, tex.Size() / 2, progress2, 0, 0);
            }
        }

        private void WindForce(int index)//wind
        {
            float sin = (float)Math.Sin(StarlightWorld.rottime + index);

            Vector2 pos = Vector2.UnitX.RotatedBy(position.ToRotation()) * 2 + Vector2.UnitY.RotatedBy(position.ToRotation()) * sin * 2.1f;

            Color color = new Color(230 - (int)(20 * sin), 120 + (int)(30 * sin), 55);

            chain.ropeSegments[index].posNow += pos;
            chain.ropeSegments[index].color = color;
        }
    }
}