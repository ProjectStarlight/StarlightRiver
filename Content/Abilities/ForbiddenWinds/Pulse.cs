using StarlightRiver.Content.Abilities.Infusions;
using System;
using Terraria.ID;

namespace StarlightRiver.Content.Abilities.ForbiddenWinds
{
	public class Pulse : InfusionItem<Dash>
	{
		public override InfusionTier Tier => InfusionTier.Untiered;
		public override string Texture => "StarlightRiver/Assets/Abilities/Pulse";
		public override string FrameTexture => "StarlightRiver/Assets/Abilities/DashFrame0";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Pulse");
			Tooltip.SetDefault("Forbidden Winds Infusion\nDash is replaced by a short, frequent, and potent burst of speed\nDecreases starlight cost by 0.25");
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 14;
			Item.rare = ItemRarityID.Green;

			color = new Color(255, 100, 220);
		}

		public override void OnActivate()
		{
			Ability.Time = 6;
			Ability.CooldownBonus -= 20;
			Ability.ActivationCostBonus -= 0.25f;
			Ability.Boost = 0.35f;
			base.OnActivate();

			// Controls how fast the Player can get before they get diminishing returns, in a 1:1 ratio to their max run speed.
			// v = 3 means they can be 3x faster than their normal max speed, etc
			const float v = 3f;

			// Calculate how fast the Player is going compared to their max run speed.
			// Allow div by zero here. With floats, it'll just return Infinity, which is fine.
			float velocityPercent = Player.velocity.Length() / (Player.maxRunSpeed * v);

			// Add to Player velocity, allowing cumulative velocity.
			// The amount of speed gained reduces in proportion to the velocityPercent. This is to prevent infinite velocity gain.
			Player.velocity += Ability.Vel / Math.Max(1, velocityPercent);

			// Reset fall
			Player.fallStart = (int)(Player.position.Y / 16);
			Player.fallStart2 = (int)(Player.position.Y / 16);
		}

		public override void UpdateActive()
		{
			if (Ability.Time-- <= 0)
				Ability.Deactivate();
		}

		public override void OnExit()
		{
		}

		public override void UpdateActiveEffects()
		{
			if (Ability.Time != 5)
				return;

			Vector2 vel = Vector2.Normalize(Player.velocity) * -1;

			const float sizeMult = 0.3f;
			const float maxSize = 1.4f;
			const float numCircles = 3.3f;

			// Iterate circle sizes
			for (float size = maxSize; size > 0; size -= maxSize / numCircles)
			{
				// Iterate dust particle in a circle
				for (float k = 0; k < 6.28f; k += 0.02f)
				{
					float ovalScale = size / 1 * sizeMult;
					float offset = size / maxSize * 30 + 10;
					Vector2 pos = Player.Center + vel * offset + (new Vector2((float)Math.Cos(k) * 20, (float)Math.Sin(k) * 40) * ovalScale).RotatedBy(Player.velocity.ToRotation());

					var d = Dust.NewDustPerfect(pos, 264, vel * (size / maxSize) * 10, 0, new Color(220, 20, 50), 0.7f);
					d.noGravity = true;
					d.noLight = true;
				}
			}
		}
	}

	class PulseImprint : InfusionImprint
	{
		public override InfusionTier Tier => InfusionTier.Bronze;
		public override string Texture => "StarlightRiver/Assets/Abilities/PulseImprint";
		public override string FrameTexture => "StarlightRiver/Assets/Abilities/DashFrame0";
		public override string PreviewVideo => "StarlightRiver/Assets/Videos/AstralPreview";

		public override int TransformTo => ModContent.ItemType<Pulse>();

		public override bool Visible => Main.LocalPlayer.controlHook;

		public override void SafeSetStaticDefaults()
		{
			DisplayName.SetDefault("Pulse");
			Tooltip.SetDefault("Dash is replaced by a short, frequent, and potent burst of speed");
		}

		public override void SetDefaults()
		{
			objectives.Add(new InfusionObjective("Implement Objectives", 1));
		}
	}
}