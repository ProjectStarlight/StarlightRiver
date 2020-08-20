using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Buffs
{
    class Squash : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Pancaked");
            Description.SetDefault("Flat ass lookin' headass");
            Main.debuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.velocity.X *= 0.9f;
        }
    }
}
