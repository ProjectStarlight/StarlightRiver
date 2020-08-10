using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Buffs
{
    class PrismaticDrown : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Prismatic Drown");
            Description.SetDefault("You are drowning in prismatic waters!");
            Main.debuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen -= 60;
            player.slow = true;
        }
    }
}
