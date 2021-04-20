using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

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

        public override void Update(Player player, ref int buffIndex)
        {
            if (!StarlightWorld.HasFlag(WorldFlags.SquidBossDowned))
            {
                player.lifeRegen -= 60;
                player.slow = true;
                player.wet = true;

                if (player == Main.LocalPlayer && Main.netMode != Terraria.ID.NetmodeID.Server)
                    Main.musicFade[Main.curMusic] = 0.05f;
            }
            else
            {
                player.wet = true;
                player.breath--;
            }
        }
    }
}
