using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Forest
{
	class OldWhetstone : SmartAccessory
    {
        public override string Texture => AssetDirectory.ForestItem + Name;

        public OldWhetstone() : base("Old Whetstone", "+1 to all damage\n'Why in tarnation are you sharpening your wand?!'") { }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.Blue;

        public override void Load()
        {
            StarlightItem.ModifyWeaponDamageEvent += AddDamage;
            return base.Autoload(ref name);
        }

		private void AddDamage(Item Item, Player Player, ref float add, ref float mult, ref float flat)
		{
            if (Equipped(Player))
                flat += 1;
		}
	}
}
