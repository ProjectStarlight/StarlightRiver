using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Forest
{
	class DustyAmulet : SmartAccessory
	{
        public override string Texture => AssetDirectory.ForestItem + Name;

        public DustyAmulet() : base("Dusty Amulet", "+20 maximum life\n+20 maximum mana\n0.8x critical strike chance\n'An old heirloom with an inscription lost to time'") { }

        public override void SafeSetDefaults() => item.rare = ItemRarityID.Blue;

        public override bool Autoload(ref string name)
        {
            StarlightItem.GetWeaponCritEvent += ReduceCrit;
            return base.Autoload(ref name);
        }

		private void ReduceCrit(Item item, Player player, ref int crit)
		{
            if(Equipped(player))
                crit = (int)(crit * 0.8f);
		}

        public override void SafeUpdateEquip(Player player)
        {
            player.statLifeMax2 += 20;
            player.statManaMax2 += 20;
        }
    }
}
