using Terraria.Localization;

namespace StarlightRiver.Content.Items.Food.Special
{
	internal class Slimejelly : BonusIngredient
	{
		public Slimejelly() : base(Language.GetTextValue("Mods.StarlightRiver.Custom.Items.Food.SpecialEffect.SlimejellyEffect")) { }

		public override FoodRecipie Recipie()
		{
			return new FoodRecipie(
			ModContent.ItemType<Slimejelly>(),
			ModContent.ItemType<Gelatine>(),
			ModContent.ItemType<GelBerry>(),
			ModContent.ItemType<StarlightWater>(),
			ModContent.ItemType<Sugar>()
			);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.slime = true;
		}
	}
}
