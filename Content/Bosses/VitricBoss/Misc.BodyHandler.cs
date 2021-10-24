using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Physics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
    class BodyHandler
    {
        private VitricBoss parent;
        private VerletChainInstance chain;
        private bool stopDrawingBody;

        public BodyHandler(VitricBoss parent)
        {
            this.parent = parent;

            Vector2 end = new Vector2(parent.arena.Center.X, parent.arena.Bottom); //WIP, dunno how this works yet - will not change anything in-game
            chain = new VerletChainInstance(9, true, parent.npc.Center, end, 5, Vector2.UnitY, false, null, true, new List<int>() { 100, 100, 48, 36, 36, 32, 32, 32, 32 });
            chain.drag = 1.1f;
        }

        public static void LoadGores()
        {
            StarlightRiver.Instance.AddGore(AssetDirectory.VitricBoss + "Gore/HeadTop", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.VitricBoss + "Gore/HeadNose", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.VitricBoss + "Gore/HeadJaw", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.VitricBoss + "Gore/HeadLeft", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.VitricBoss + "Gore/HeadRight", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.VitricBoss + "Gore/CheekLeft", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.VitricBoss + "Gore/CheekRight", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.VitricBoss + "Gore/HornLeft", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.VitricBoss + "Gore/HornRight", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.VitricBoss + "Gore/BodyTop", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.VitricBoss + "Gore/BodyBottom", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.VitricBoss + "Gore/SegmentLarge", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.VitricBoss + "Gore/SegmentMedium", new DebugGore());
            StarlightRiver.Instance.AddGore(AssetDirectory.VitricBoss + "Gore/SegmentSmall", new DebugGore());
        }

        public void DrawBody(SpriteBatch sb)
        {
            chain.DrawRope(sb, DrawSegment);

            if (parent.Phase == (int)VitricBoss.AIStates.FirstPhase && parent.npc.dontTakeDamage) //draws the npc's shield when immune and in the first phase
                chain.DrawRope(sb, DrawShield);
        }

        private void DrawSegment(SpriteBatch sb, int index, Vector2 pos)
        {
            if (stopDrawingBody && index > 0)
                return;

            var tex = GetTexture(AssetDirectory.VitricBoss + "VitricBossBody");
            var glowTex = GetTexture(AssetDirectory.VitricBoss + "VitricBossBodyGlow");

            float rot = 0;

            if (index != 0)
                rot = (chain.ropeSegments[index].posNow - chain.ropeSegments[index - 1].posNow).ToRotation() - (float)Math.PI / 2;

            Rectangle source;

            switch (index)
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
                shouldBeTwistFrame = thisTimer < threshold - diff || thisTimer > threshold + diff;
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

            if (index == 0 && parent.Phase != (int)VitricBoss.AIStates.Dying && 
                parent.Phase != (int)VitricBoss.AIStates.SpawnAnimation) //change this so the head is drawn from here later maybe? and move all visuals into this class?
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

                if (parent.npc.frame.Y == parent.npc.frame.Height * 2 || parent.npc.frame.Y == parent.npc.frame.Height * 3)
                {
                    parent.npc.frame.Y = parent.npc.frame.Height * (shouldBeTwistFrame ? 2 : 3);
                }
            }

            var brightness = (0.5f + (float)Math.Sin(StarlightWorld.rottime + index));

            if (index == 1)
                brightness += 0.25f;

            sb.Draw(tex, pos - Main.screenPosition, source, Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16), rot, source.Size() / 2, 1, flip, 0);
            sb.Draw(glowTex, pos - Main.screenPosition, source, Color.White * brightness, rot, source.Size() / 2, 1, flip, 0);

            var tile = Framing.GetTileSafely((int)pos.X / 16, (int)pos.Y / 16);

            if (!tile.active() && tile.wall == 0)
                Lighting.AddLight(pos, new Vector3(1, 0.8f, 0.2f) * brightness * 0.4f);
        }

        private void DrawShield(SpriteBatch sb, int index, Vector2 pos)
        {
            if (index == 0)
                return;

            var tex = GetTexture(AssetDirectory.VitricBoss + "VitricBossBodyShield");

            float rot = (chain.ropeSegments[index].posNow - chain.ropeSegments[index - 1].posNow).ToRotation() - (float)Math.PI / 2;

            Rectangle source;

            switch (index)
            {
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
                shouldBeTwistFrame = thisTimer < threshold - diff || thisTimer > threshold + diff;
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

            var effect = Terraria.Graphics.Effects.Filters.Scene["MoltenForm"].GetShader().Shader;
            effect.Parameters["sampleTexture2"].SetValue(GetTexture("StarlightRiver/Assets/Bosses/VitricBoss/ShieldMap"));
            effect.Parameters["uTime"].SetValue(2 - (parent.shieldShaderTimer / 120f) * 2);
            effect.Parameters["sourceFrame"].SetValue(new Vector4(source.X, source.Y, source.Width, source.Height));
            effect.Parameters["texSize"].SetValue(tex.Size());

            sb.End();
            sb.Begin(default, BlendState.NonPremultiplied, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

            sb.Draw(tex, pos - Main.screenPosition, source, Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16), rot, source.Size() / 2, 1, flip, 0);

            sb.End();
            sb.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
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

        public void SpawnGores2()
		{
            var pos = chain.ropeSegments[0].posNow;

            GoreMe(pos, new Vector2(-60, -30), AssetDirectory.VitricBoss + "Gore/HeadLeft");
            GoreMe(pos, new Vector2(60, -30), AssetDirectory.VitricBoss + "Gore/HeadRight");

            GoreMe(pos, new Vector2(-60, 0), AssetDirectory.VitricBoss + "Gore/HornLeft");
            GoreMe(pos, new Vector2(60, 0), AssetDirectory.VitricBoss + "Gore/HornRight");

            GoreMe(pos, new Vector2(0, -15), AssetDirectory.VitricBoss + "Gore/HeadTop");
            GoreMe(pos, new Vector2(0, 0), AssetDirectory.VitricBoss + "Gore/HeadNose");
            GoreMe(pos, new Vector2(0, 80), AssetDirectory.VitricBoss + "Gore/HeadJaw");

            GoreMe(pos, new Vector2(-45, 40), AssetDirectory.VitricBoss + "Gore/CheekLeft");
            GoreMe(pos, new Vector2(45, 40), AssetDirectory.VitricBoss + "Gore/CheekRight");
        }

        public void SpawnGores()
		{
            stopDrawingBody = true;

            for (int k = 0; k < chain.ropeSegments.Count; k++)
			{
                var pos = chain.ropeSegments[k].posNow;

                switch (k)
                {
                    case 1:
                        GoreMe(pos, new Vector2(0, 0), AssetDirectory.VitricBoss + "Gore/BodyTop");
                        GoreMe(pos, new Vector2(0, 50), AssetDirectory.VitricBoss + "Gore/BodyBottom");
                        break;

                    case 2:
                        GoreMe(pos, new Vector2(0, 0), AssetDirectory.VitricBoss + "Gore/SegmentLarge");
                        break;

                    case 3:
                    case 4:
                        GoreMe(pos, new Vector2(0, 0), AssetDirectory.VitricBoss + "Gore/SegmentMedium");
                        break;

                    default:
                        GoreMe(pos, new Vector2(0, 0), AssetDirectory.VitricBoss + "Gore/SegmentSmall");
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
	}
}
