using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs
{
	internal class TestingGuy : ModNPC
    {
        public override string Texture => AssetDirectory.VitricNpc + "CrystalSlime";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Test Subject");
            Main.npcFrameCount[NPC.type] = 2;
        }

        public override void SetDefaults()
        {
            NPC.width = 48;
            NPC.height = 32;
            NPC.damage = 10;
            NPC.defense = 5;
            NPC.lifeMax = 2500;
            NPC.HitSound = SoundID.NPCHit42;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 99999999f;
            NPC.knockBackResist = 0.6f;
            NPC.aiStyle = 1;

            NPC.GetGlobalNPC<ShieldNPC>().MaxShield = 100;
        }
    }
}