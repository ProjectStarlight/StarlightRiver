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
			Tooltip.SetDefault("+30% Inoculation");
		}

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 30;
			Item.maxStack = 30;
			Item.useStyle = Terraria.ID.ItemUseStyleID.EatFood;
			Item.consumable = true;
			Item.buffType = ModContent.BuffType<InoculationPotionBuff>();
			Item.buffTime = 180 * 60;
			Item.UseSound = Terraria.ID.SoundID.Item3;
		}
	}

	class InoculationPotionBuff : ModBuff
	{
		public override string Texture => AssetDirectory.PotionsItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Inoculated");
			Description.SetDefault("+30% Inoculation");
		}

		public override void Update(Player Player, ref int buffIndex)
		{
			Player.GetModPlayer<DoTResistancePlayer>().DoTResist += 0.3f;
		}
	}
}
