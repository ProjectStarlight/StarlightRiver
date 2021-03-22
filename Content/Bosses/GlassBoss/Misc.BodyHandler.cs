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
        private VitricBoss parent;
        private VerletChainInstance chain;

        public BodyHandler(VitricBoss parent)
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
            var shapeTex = GetTexture(AssetDirectory.GlassBoss + "VitricBossBodyShape");

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

            int thisTimer = parent.twistTimer;
            int threshold = (int)(parent.maxTwistTimer / 8f * (index + 1)) - 1;

            bool shouldBeTwistFrame = false;
            if (Math.Abs(parent.lastTwistState - parent.twistTarget) == 2) //flip from 1 side to the other
            {
                int diff = parent.maxTwistTimer / 8 / 3;
                shouldBeTwistFrame = thisTimer < threshold - diff|| thisTimer > threshold + diff;
            }

            if (Math.Abs(parent.twistTarget) == 1) //flip from front to side
                shouldBeTwistFrame = thisTimer > threshold;
            else if (parent.twistTarget == 0) //flip from side to front
                shouldBeTwistFrame = thisTimer < threshold;

            SpriteEffects flip = (
                (parent.twistTarget == -1 && parent.twistTimer > threshold) ||
                (parent.lastTwistState == -1 && parent.twistTimer < threshold)
                ) && shouldBeTwistFrame ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (shouldBeTwistFrame)
                source.X += 114;

            if(index == 0) //change this so the head is drawn from here later maybe? and move all visuals into this class?
            {
                parent.npc.spriteDirection = (int)flip;

                if (shouldBeTwistFrame) //this is dumb, mostly just to test
                    parent.npc.frame.X = parent.npc.frame.Width;
                else
                    parent.npc.frame.X = 0;
            }

            var brightness = (0.5f + (float)Math.Sin(StarlightWorld.rottime + index));

            if (index == 1) brightness += 0.25f;

            if (parent.Phase == (int)VitricBoss.AIStates.LastStand && parent.GlobalTimer >= 140) //fuck you immediate rendering fuck you fuck you fuck you
                source.Y += 232;

            sb.Draw(tex, pos - Main.screenPosition, source, Lighting.GetColor((int)pos.X  / 16, (int)pos.Y / 16), rot, source.Size() / 2, 1, flip, 0);
            sb.Draw(glowTex, pos - Main.screenPosition, source, Color.White * brightness, rot, source.Size() / 2, 1, flip, 0);

            if (parent.Phase == (int)VitricBoss.AIStates.LastStand) //fuck you immediate rendering fuck you fuck you fuck you
                sb.Draw(shapeTex, pos - Main.screenPosition, new Rectangle(source.X, source.Y % 232, source.Width, source.Height), parent.glowColor, rot, source.Size() / 2, 1, flip, 0);

            Lighting.AddLight(pos, new Vector3(1, 0.8f, 0.2f) * brightness * 0.4f);
        }

        public void UpdateBody()
        {
            chain.UpdateChain(parent.npc.Center);
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
