using StarlightRiver.Content.Abilities.Faewhip;
using StarlightRiver.Content.Abilities.Infusions;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Tiles.Vitric.Temple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Abilities.ForbiddenWinds
{
	internal class GatheringWinds : Dash
	{
		public int charge;

		public override float ActivationCostDefault => 0f;

		public override void OnActivate()
		{
			base.OnActivate();		

			Time = 25;
		}

		public override void UpdateActive()
		{
			bool control = StarlightRiver.Instance.AbilityKeys.Get<Whip>().Current;

			if (Time >= 24 && control && Player.GetHandler().Stamina > 0.1f)
			{
				Time = 25;
				charge++;
				Player.GetHandler().Stamina -= 0.016f;
			}
			else
			{
				if (Time == 23)
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Item96.WithPitchOffset(charge / 60f), Player.Center);

				Player.velocity = Dash.SignedLesserBound(Dir * charge, Player.velocity); // "conservation of momentum"

				Player.frozen = true;
				Player.gravity = 0;
				Player.maxFallSpeed = Math.Max(Player.maxFallSpeed, Speed);

				if (Time-- <= 0)
					Deactivate();
			}
		}

		public override void Reset()
		{
			Boost = 0.5f;
			Speed = 22;
			Time = maxTime = 25;
			CooldownBonus = 0;
		}

		public override void UpdateActiveEffects()
		{
			if (Time == 25)
				return;

			Vector2 nextPos = Player.Center + Vector2.Normalize(Player.velocity) * Speed;
			for (float k = 0; k <= 1; k += 0.08f)
			{
				Vector2 swirlOff = Vector2.UnitX.RotatedBy(Player.velocity.ToRotation() + 1.57f) * (float)Math.Sin((Time + k) / 25f * 6.28f * 1.5f) * 14;
				Vector2 pos = Player.Center + (Player.Center - nextPos) * k + swirlOff;
				Vector2 vel = Player.velocity * Main.rand.NextFloat(0.1f, 0.2f) + swirlOff * Main.rand.NextFloat(0.1f, 0.14f);

				int type = k == 0 ? ModContent.DustType<Dusts.AuroraDecelerating>() : ModContent.DustType<Dusts.Cinder>();

				if (type == ModContent.DustType<Dusts.AuroraDecelerating>())
					vel *= 4;

				var d = Dust.NewDustPerfect(pos, type, vel, 0, new Color(40, 230 - (int)(Time / 25f * 160), 255), Main.rand.NextFloat(0.2f, 0.4f));
				d.customData = Main.rand.NextFloat(0.4f, 0.9f);
			}
		}

		public override void UpdateFixed()
		{
			base.UpdateFixed();

			if (cache is null)
				return;

			if (EffectTimer < 44 - 24)
			{
				for (int i = 0; i < 24; i++)
				{
					Vector2 swirlOff2 = Vector2.UnitX.RotatedBy((cache[0] - cache[23]).ToRotation() + 1.57f) * (float)Math.Sin((i - 3) / 25f * 6.28f * 1.5f) * 30;
					cache[i] += swirlOff2 * 0.05f;
					cache[i] += Vector2.Normalize(cache[23] - cache[0]) * 1f;
				}
			}
		}
	}

	class GatheringWindsItem : InfusionItem<Dash, GatheringWinds>
	{
		public override InfusionTier Tier => InfusionTier.Bronze;
		public override string Texture => "StarlightRiver/Assets/Abilities/Astral";
		public override string FrameTexture => "StarlightRiver/Assets/Abilities/DefaultFrame";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gathering Winds");
			Tooltip.SetDefault("Forbidden Winds Infusion\nHold the dash key to hover and charge up a dash\nDrain 1 starlight/s while charging");
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
			recipe.AddTile(ModContent.TileType<MainForge>());
			recipe.Register();
		}
	}
}
