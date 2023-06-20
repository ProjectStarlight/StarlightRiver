using StarlightRiver.Content.Items.BaseTypes;
using System;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class Cheapskates : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public Cheapskates() : base("Cheapskates", "Maximum movement speed is doubled\nTake up to 25% more damage while moving over your previous max speed\nAcceleration is reduced by 75% when over your previous max speed") { }

		public override void Load()
		{
			StarlightPlayer.ModifyHurtEvent += PreHurtAccessory;
			StarlightPlayer.PostUpdateRunSpeedsEvent += ModifyMovement;
		}

		public override void Unload()
		{
			StarlightPlayer.ModifyHurtEvent -= PreHurtAccessory;
			StarlightPlayer.PostUpdateRunSpeedsEvent -= ModifyMovement;
		}

		private void PreHurtAccessory(Player player, ref Player.HurtModifiers modifiers)
		{
			if (Equipped(player) && Math.Abs(player.velocity.X) > 3f)
			{
				float damageMult = 1f + MathHelper.Lerp(0, 0.25f, (Math.Abs(player.velocity.X) - 3f) / (player.maxRunSpeed * 0.5f));
				modifiers.SourceDamage *= damageMult;
			}
		}

		private void ModifyMovement(Player Player)
		{
			if (Equipped(Player))
			{
				Player.maxRunSpeed *= 2;

				if (Math.Abs(Player.velocity.X) > 3f)
					Player.runAcceleration *= 0.25f;
			}
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 1);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();

			recipe.AddIngredient(ItemID.Wood, 50);
			recipe.AddIngredient(ItemID.Chain, 10);
			recipe.AddIngredient(ItemID.DemoniteBar, 20);
			recipe.AddTile(TileID.IceMachine);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Wood, 50);
			recipe.AddIngredient(ItemID.Chain, 10);
			recipe.AddIngredient(ItemID.CrimtaneBar, 20);
			recipe.AddTile(TileID.IceMachine);
			recipe.Register();
		}
	}
}