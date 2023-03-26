using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Forest
{
	class OldWhetstone : SmartAccessory
	{
		public override string Texture => AssetDirectory.ForestItem + Name;

		public OldWhetstone() : base("Old Whetstone", "+1 to all damage\n'Why in tarnation are you sharpening your wand?!'") { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 0, 20, 0);
		}

		public override void Load()
		{
			StarlightItem.ModifyWeaponDamageEvent += AddDamage;
		}

		public override void Unload()
		{
			StarlightItem.ModifyWeaponDamageEvent -= AddDamage;
		}

		private void AddDamage(Item Item, Player Player, ref StatModifier statModifier)
		{
			if (Equipped(Player))
				statModifier.Flat += 1;
		}
	}
}
