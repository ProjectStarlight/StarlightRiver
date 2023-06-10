using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Forest
{
	class DustyAmulet : SmartAccessory
	{
		public override string Texture => AssetDirectory.ForestItem + Name;

		public DustyAmulet() : base("Dusty Amulet", "+20 maximum life\n+20 maximum mana\n0.8x critical strike chance\n'An old heirloom with an inscription lost to time'") { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;

			Item.value = Item.sellPrice(silver: 15);
		}

		public override void Load()
		{
			StarlightItem.GetWeaponCritEvent += ReduceCrit;
		}

		public override void Unload()
		{
			StarlightItem.GetWeaponCritEvent -= ReduceCrit;
		}

		private void ReduceCrit(Item Item, Player Player, ref float crit)
		{
			if (Equipped(Player))
				crit = (int)(crit * 0.8f);
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.statLifeMax2 += 20;
			Player.statManaMax2 += 20;
		}
	}
}