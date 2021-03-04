using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    class BodyHandler
    {
        private NPC parent;
        private VerletChainInstance chain;

        public BodyHandler(NPC parent)
        {
            this.parent = parent;

            chain = new VerletChainInstance(true);
            chain.segmentCount = 8;
            chain.customDistances = true;
            chain.drag = 1.1f;
            chain.segmentDistanceList = new List<float>() {100, 100, 48, 36, 32, 32, 32, 32 };
        }

        public void DrawBody(SpriteBatch sb)
        {
            chain.DrawRope(sb, DrawSegment);
        }

        private void DrawSegment(SpriteBatch sb, int index, Vector2 pos)
        {
            var tex = GetTexture(AssetDirectory.GlassBoss + "VitricBossBody");
            var glowTex = GetTexture(AssetDirectory.GlassBoss + "VitricBossBodyGlow");

            float rot = 0;

            if(index != 0)
                rot = (chain.ropeSegments[index].posNow - chain.ropeSegments[index - 1].posNow).ToRotation() - (float)Math.PI / 2;

            Rectangle source;

            switch(index)
            {
                case 0: source = new Rectangle(0, 0, 0, 0); break;
                case 1: source = new Rectangle(0, 0, 114, 114); break;
                case 2: source = new Rectangle(18, 120, 78, 44); break;
                case 3: source = new Rectangle(26, 168, 62, 30); break;
                default: source = new Rectangle(40, 204, 34, 28); break;
            }

            var brightness = (0.5f + (float)Math.Sin(StarlightWorld.rottime + index));

            if (index == 1) brightness += 0.25f;

            sb.Draw(tex, pos - Main.screenPosition, source, Lighting.GetColor((int)pos.X  / 16, (int)pos.Y / 16), rot, source.Size() / 2, 1, 0, 0);
            sb.Draw(glowTex, pos - Main.screenPosition, source, Color.White * brightness, rot, source.Size() / 2, 1, 0, 0);

            Lighting.AddLight(pos, new Vector3(1, 0.8f, 0.2f) * brightness * 0.4f);
        }

        public void UpdateBody()
        {
            chain.UpdateChain(parent.Center);
            chain.IterateRope(updateBodySegment);
        }

        private void updateBodySegment(int index)
        {
            if (index == 1)
                chain.ropeSegments[index].posNow.Y += 20;

            if (index == 2)
                chain.ropeSegments[index].posNow.Y += 10;

            if (index == 3)
                chain.ropeSegments[index].posNow.Y += 5;

            chain.ropeSegments[index].posNow.X += (float)Math.Sin(Main.GameUpdateCount / 40f + (index * 0.8f)) * 0.5f;
        }
    }
}
