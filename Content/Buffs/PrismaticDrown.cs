using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Buffs
{
	class PrismaticDrown : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Buffs + name;
            return true;
        }

        public override void SetDefaults()
        {
            DisplayName.SetDefault("Prismatic Drown");
            Description.SetDefault("You are drowning in prismatic waters!");
            Main.debuff[Type] = true;
        }

        public override void Update(Player Player, ref int buffIndex)
        {
            if (!StarlightWorld.HasFlag(WorldFlags.SquidBossDowned))
            {
                Player.lifeRegen -= 60;
                Player.slow = true;
                Player.wet = true;

                if (Player == Main.LocalPlayer && Main.netMode != Terraria.ID.NetmodeID.Server)
                    Main.musicFade[Main.curMusic] = 0.05f;
            }
            else
            {
                Player.wet = true;
                Player.breath--;
            }
        }
    }
}
