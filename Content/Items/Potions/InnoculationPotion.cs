using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Potions
{
	class InoculationPotion : ModItem
	{
		public override string Texture => AssetDirectory.PotionsItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Inoculation Potion");
			Tooltip.SetDefault("+30% DoT resistance");
		}

		public override void SetDefaults()
		{
			item.width = 18;
			item.height = 30;
			item.maxStack = 30;
			item.useStyle = Terraria.ID.ItemUseStyleID.EatingUsing;
			item.consumable = true;
			item.buffType = ModContent.BuffType<InoculationPotionBuff>();
			item.buffTime = 180 * 60;
			item.UseSound = Terraria.ID.SoundID.Item3;
		}
	}

	class InoculationPotionBuff : ModBuff
	{
		public override bool Autoload(ref string name, ref string texture)
		{
			texture = AssetDirectory.PotionsItem + name;
			return base.Autoload(ref name, ref texture);
		}

		public override void SetDefaults()
		{
			DisplayName.SetDefault("Inoculated");
			Description.SetDefault("+30% to DoT Resistance");
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.GetModPlayer<DoTResistancePlayer>().DoTResist += 0.3f;
		}
	}
}
