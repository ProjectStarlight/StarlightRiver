using StarlightRiver.Content.Abilities.Infusions;
using System;
using Terraria.ID;

namespace StarlightRiver.Content.Abilities.ForbiddenWinds
{
	public class Blink : InfusionItem<Dash>
	{
		public override InfusionTier Tier => InfusionTier.Untiered;
		public override string Texture => "StarlightRiver/Assets/Abilities/Blink";
		public override string FrameTexture => "StarlightRiver/Assets/Abilities/DashFrame0";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Blink");
			Tooltip.SetDefault("Forbidden Winds Infusion\nInstantly teleport a short distance ahead, avoiding enemies and their attacks");
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 14;
			Item.rare = ItemRarityID.Green;

			color = new Color(200, 200, 220);
		}

		private const int maxTime = 4;
		private Vector2 stored;

		public override void OnActivate()
		{
			Ability.Time = maxTime;
			Ability.Speed = 40;
			Ability.Boost = 0;
			Ability.StartCooldown();

			// Do this to ensure the Player has velocity when they dash (for compatibility with dash checks etc)
			if (Player.velocity.Y == 0)
				Player.velocity.Y = -0.1f;

			// Store Player velocity for exiting dash
			stored = Player.velocity;
		}

		public override void UpdateActive()
		{
			if (Ability.Time == maxTime)
			{
				// Local client effects
				if (Player.whoAmI == Main.myPlayer)
					Main.SetCameraLerp(0.2f, 5);

				// Enter tp velocity
				Player.velocity = Ability.Dir * Ability.Speed * Ability.Time;
				Player.maxFallSpeed = Player.velocity.Y;
				Player.frozen = true;

				// Disable grapples
				Player.grappling[0] = -1;
				Player.grapCount = 0;
				for (int i = 0; i < 1000; i++)
				{
					if (Main.projectile[i].active && Main.projectile[i].owner == Player.whoAmI && Main.projectile[i].aiStyle == 7)
						Main.projectile[i].Kill();
				}
			}
			else if (Ability.Time == maxTime - 1)
			{
				// Cancel tp velocity
				Player.velocity = stored;
			}

			// Do fx at old then new position
			if (Ability.Time > maxTime - 2 && Main.netMode != NetmodeID.Server)
				TeleportFx(Player.Center, Ability.Time == maxTime);

			if (Ability.Time <= 0)
				Ability.Deactivate();

			Ability.Time--;
		}

		private void TeleportFx(Vector2 position, bool start)
		{
			// Animation plays at new and old position
			const int speed = 6;
			const int dustCount = 60;
			for (int i = 0; i < dustCount; i++)
			{
				Vector2 pos = position;
				Vector2 vel = Vector2.UnitX.RotatedBy(Math.PI * 2 * i / dustCount) * speed;
				if (start)
				{
					pos += vel * 10;
					vel *= -1;
				}

				var d = Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.AirDash>(), vel);
				d.scale = 1.75f;
				d.fadeIn = 7;
			}

			if (start)
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15, position);
		}

		public override void OnExit()
		{
			// DO NOT add fall damage reset!
			// This ability does nothing to your velocity. It shouldn't.
		}

		public override void UpdateActiveEffects()
		{
			// No visuals from the original.
		}
	}

	class BlinkImprint : InfusionImprint
	{
		public override InfusionTier Tier => InfusionTier.Bronze;
		public override string Texture => "StarlightRiver/Assets/Abilities/BlinkImprint";
		public override string FrameTexture => "StarlightRiver/Assets/Abilities/DashFrame0";
		public override string PreviewVideo => "StarlightRiver/Assets/Videos/AstralPreview";

		public override int TransformTo => ModContent.ItemType<Blink>();

		public override void SafeSetStaticDefaults()
		{
			DisplayName.SetDefault("Blink");
			Tooltip.SetDefault("Instantly teleport a short distance ahead, avoiding enemies and their attacks");
		}

		public override void SetDefaults()
		{
			objectives.Add(new InfusionObjective("Implement Objectives", 1));
		}
	}
}
