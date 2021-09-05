using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Physics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
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

            Vector2 end = new Vector2(parent.arena.Center.X, parent.arena.Bottom); //WIP, dunno how this works yet - will not change anything in-game
            chain = new VerletChainInstance(9, true, parent.npc.Center, end, 5, Vector2.UnitY, false, null, true, new List<int>() { 100, 100, 48, 36, 36, 32, 32, 32, 32 });
            chain.drag = 1.1f;
        }

        public static void LoadGores()
		{
            StarlightRiver.Instance.AddGore(AssetDirectory.GlassBoss + "Gore/HeadTop", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.GlassBoss + "Gore/HeadNose", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.GlassBoss + "Gore/HeadJaw", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.GlassBoss + "Gore/HeadLeft", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.GlassBoss + "Gore/HeadRight", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.GlassBoss + "Gore/CheekLeft", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.GlassBoss + "Gore/CheekRight", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.GlassBoss + "Gore/HornLeft", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.GlassBoss + "Gore/HornRight", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.GlassBoss + "Gore/BodyTop", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.GlassBoss + "Gore/BodyBottom", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.GlassBoss + "Gore/SegmentLarge", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.GlassBoss + "Gore/SegmentMedium", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.GlassBoss + "Gore/SegmentSmall", new DebugGore());
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

                case 3:
                case 4:
                    source = new Rectangle(26, 168, 62, 30); break;

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
                (parent.lastTwistState == -1)
                ) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (Math.Abs(parent.lastTwistState) == Math.Abs(parent.twistTarget) && parent.lastTwistState != parent.twistTarget)
			{
                int dir = (int)flip;
                dir *= (parent.twistTimer > parent.maxTwistTimer / 2 ? 1 : -1);

                flip = dir == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			}

            if (shouldBeTwistFrame)
                source.X += 114;

            if (index == 0) //change this so the head is drawn from here later maybe? and move all visuals into this class?
            {
                parent.npc.spriteDirection = (int)flip;

                if (parent.npc.frame.Y == 0)
                {
                    if (Math.Abs(parent.lastTwistState) == Math.Abs(parent.twistTarget) && parent.lastTwistState != parent.twistTarget)
                    {
                        parent.npc.frame.X = parent.npc.frame.Width * (2 - (int)(Math.Sin(parent.twistTimer / (float)parent.maxTwistTimer * 3.14f) * 2));
                    }

                    else if (shouldBeTwistFrame) //this is dumb, mostly just to test
                        parent.npc.frame.X = parent.npc.frame.Width * (int)((parent.twistTimer / (float)parent.maxTwistTimer) * 2);
                    else
                        parent.npc.frame.X = parent.npc.frame.Width * (2 - (int)((parent.twistTimer / (float)parent.maxTwistTimer) * 2));
                }
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
            chain.UpdateChain(parent.npc.Center + parent.PainOffset);
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

        public void SpawnGores()
		{
            for (int k = 0; k < chain.ropeSegments.Count; k++)
			{
                var pos = chain.ropeSegments[k].posNow;

                switch (k)
                {
                    case 0:
                        GoreMe(pos, new Vector2(0, -15), AssetDirectory.GlassBoss + "Gore/HeadTop");
                        GoreMe(pos, new Vector2(0, 0), AssetDirectory.GlassBoss + "Gore/HeadNose");
                        GoreMe(pos, new Vector2(0, 80), AssetDirectory.GlassBoss + "Gore/HeadJaw");

                        GoreMe(pos, new Vector2(-60, -30), AssetDirectory.GlassBoss + "Gore/HeadLeft");
                        GoreMe(pos, new Vector2(60, -30), AssetDirectory.GlassBoss + "Gore/HeadRight");

                        GoreMe(pos, new Vector2(-45, 40), AssetDirectory.GlassBoss + "Gore/CheekLeft");
                        GoreMe(pos, new Vector2(45, 40), AssetDirectory.GlassBoss + "Gore/CheekRight");

                        GoreMe(pos, new Vector2(-60, 0), AssetDirectory.GlassBoss + "Gore/HornLeft");
                        GoreMe(pos, new Vector2(60, 0), AssetDirectory.GlassBoss + "Gore/HornRight");

                        break;

                    case 1:
                        GoreMe(pos, new Vector2(0, 0), AssetDirectory.GlassBoss + "Gore/BodyTop");
                        GoreMe(pos, new Vector2(0, 50), AssetDirectory.GlassBoss + "Gore/BodyBottom");
                        break;

                    case 2:
                        GoreMe(pos, new Vector2(0, 0), AssetDirectory.GlassBoss + "Gore/SegmentLarge");
                        break;

                    case 3:
                    case 4:
                        GoreMe(pos, new Vector2(0, 0), AssetDirectory.GlassBoss + "Gore/SegmentMedium");
                        break;

                    default:
                        GoreMe(pos, new Vector2(0, 0), AssetDirectory.GlassBoss + "Gore/SegmentSmall");
                        break;
                }
            }
		}

        private void GoreMe(Vector2 pos, Vector2 offset, string tex)
		{
            var texture = GetTexture(tex);
            Gore.NewGorePerfect(pos + offset - texture.Size() / 2, offset == Vector2.Zero ? Vector2.One.RotatedByRandom(6.28f) : Vector2.Normalize(offset) * Main.rand.NextFloat(2, 4), ModGore.GetGoreSlot(tex));
        }
    }

    class DebugGore : ModGore
	{
		public override void OnSpawn(Gore gore)
		{
            gore.timeLeft = 30;
		}

		public override bool Update(Gore gore)
		{
            return true;

            gore.timeLeft -= 5;
            gore.active = gore.timeLeft > 0;
            return false;
		}
	}
}
