using Terraria;
using Terraria.ID;

namespace StarlightRiver.Items.Accessories
{
    public class MirageBoots : SmartAccessory
    {
        public MirageBoots() : base("Mirage Boots", "NaN") { }
        public override void SafeSetDefaults() { item.rare = ItemRarityID.Orange; }
        public override void SafeUpdateEquip(Player player)
        {
            player.rocketBoots = 4;
            player.rocketTimeMax = 10;
            if (player.rocketFrame)
            {
            }
            //if (player.velocity.Y == 0) player.rocketTime = 600;
            //Main.NewText("Rocket Power: " + player.rocketTime + "/" + player.rocketTimeMax);
        }
    }
}