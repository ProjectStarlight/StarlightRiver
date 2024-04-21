using StarlightRiver.Content.Abilities.Infusions;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Tiles.Crafting;
using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Abilities.ForbiddenWinds
{
	class StellarRush : Dash
	{
		public override void OnActivate()
		{
			Speed *= 0.95f;
			Boost = 0.5f;
			ActivationCostBonus += 0.5f;

			base.OnActivate();
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item96, Player.Center);

			Time = 15;
		}

		public override void UpdateActive()
		{
			Player.velocity = Dash.SignedLesserBound(Dir * Speed, Player.velocity); // "conservation of momentum"

			Player.frozen = true;
			Player.gravity = 0;
			Player.maxFallSpeed = Math.Max(Player.maxFallSpeed, Speed);

			if (Time-- <= 0)
				Deactivate();
		}

		public override void UpdateActiveEffects()
		{
			Vector2 nextPos = Player.Center + Vector2.Normalize(Player.velocity) * Speed;
			for (float k = -2; k <= 2; k += 0.1f)
			{
				Vector2 pos = nextPos + Vector2.UnitX.RotatedBy(Player.velocity.ToRotation() + k) * 7 * (4 - Time);

				Dust.NewDustPerfect(pos, DustType<Dusts.BlueStamina>(), Player.velocity * Main.rand.NextFloat(-0.4f, 0), 0, default, 1 - Time / 15f);

				if (Math.Abs(k) >= 1.5f)
					Dust.NewDustPerfect(pos, DustType<Dusts.BlueStamina>(), Player.velocity * Main.rand.NextFloat(-0.6f, -0.4f), 0, default, 2.2f - Time / 15f);
			}
		}
	}

	class StellarRushItem : InfusionItem<Dash, StellarRush>
	{
		public override InfusionTier Tier => InfusionTier.Bronze;
		public override string Texture => "StarlightRiver/Assets/Abilities/Astral";
		public override string FrameTexture => "StarlightRiver/Assets/Abilities/DefaultFrame";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Stellar Rush");
			Tooltip.SetDefault("Forbidden Winds Infusion\nDash farther and carry more speed\nIncreases starlight cost to 1.5");
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 14;
			Item.rare = ItemRarityID.Green;

			color = new Color(100, 200, 250);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<BasicInfusion>(1);
			recipe.AddIngredient<StaminaGel>(25);
			recipe.AddTile<Infuser>();
			recipe.Register();
		}
	}
}