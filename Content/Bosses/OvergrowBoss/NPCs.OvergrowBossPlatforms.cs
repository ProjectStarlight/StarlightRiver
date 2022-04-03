using Microsoft.Xna.Framework;
using StarlightRiver.Content.NPCs.BaseTypes;
using System;
using Terraria;

namespace StarlightRiver.NPCs.Boss.OvergrowBoss
{
	internal class OvergrowBossVerticalPlatform : MovingPlatform
    {
        public override string Texture => "StarlightRiver/Assets/Bosses/OvergrowBoss/OvergrowBossPlatform";

        public override void SafeSetDefaults()
        {
            NPC.width = 100;
            NPC.height = 16;
        }

        public override void SafeAI()
        {
            NPC.ai[0] += 0.04f;
            if (NPC.ai[0] > 6.28f) NPC.ai[0] = 0;
            NPC.velocity.Y = (float)Math.Sin(NPC.ai[0]) * 3;
        }
    }

    internal class OvergrowBossCircularPlatform : MovingPlatform
    {
        public override string Texture => "StarlightRiver/Assets/Bosses/OvergrowBoss/OvergrowBossPlatform";

        public override void SafeSetDefaults()
        {
            NPC.width = 100;
            NPC.height = 16;
        }

        public override void SafeAI()
        {
            NPC.ai[0] += 0.04f;
            if (NPC.ai[0] > 6.28f) NPC.ai[0] = 0;
            NPC.velocity += new Vector2(1, 0).RotatedBy(NPC.ai[0]) * 0.1f;
        }
    }
}