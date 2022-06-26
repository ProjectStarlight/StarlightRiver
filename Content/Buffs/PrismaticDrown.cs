using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Buffs
{
	class PrismaticDrown : ModBuff
    {
        public override string Texture => AssetDirectory.Buffs + "PrismaticDrown";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Prismatic Drown");
            Description.SetDefault("You are drowning in prismatic waters!");
            Main.debuff[Type] = true;
        }

        public override void Update(Player Player, ref int buffIndex)
        {
            if (NPC.AnyNPCs(ModContent.NPCType<Content.Bosses.SquidBoss.SquidBoss>()))
            {
                Player.lifeRegen -= Main.masterMode ? 100 : 60;

                if (Player == Main.LocalPlayer && Main.netMode != Terraria.ID.NetmodeID.Server)
                    Main.musicFade[Main.curMusic] = 0.05f;
            }
            else
            {
                Player.breath--;
            }
        }

		public override void Update(NPC npc, ref int buffIndex)
		{
            npc.lifeRegen -= 40;
		}
	}
}
