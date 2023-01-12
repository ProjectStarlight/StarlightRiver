using StarlightRiver.Content.Abilities.Infusions;
using System;
using System.Collections.Generic;
using Terraria.ID;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using StarlightRiver.Content.Dusts;

namespace StarlightRiver.Content.Abilities.ForbiddenWinds
{
	public class WindTunnel : InfusionItem<Dash>
	{
		private Projectile tunnel;

		public override InfusionTier Tier => InfusionTier.Untiered;

		public override string Texture => "StarlightRiver/Assets/Abilities/Pulse";

		public override string FrameTexture => "StarlightRiver/Assets/Abilities/DashFrame0";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Wind Tunnel");
			Tooltip.SetDefault("Forbidden Winds Infusion\nInstead of dashing, place a wind tunnel\nThis wind tunnel propels you if you travel through it");
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
			//tunnel?.Kill();
			tunnel = Projectile.NewProjectileDirect(Item.GetSource_Misc("Infusion"), Main.MouseWorld, Vector2.Zero, ModContent.ProjectileType<WindTunnelProj>(), 0, 0);
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

		}
	}

	public class WindTunnelProj : ModProjectile
	{
		public override string Texture => AssetDirectory.Debug;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Wind Tunnel");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.timeLeft = 900;
			Projectile.width = Projectile.height = 48;
		}

		public override void AI()
		{
			Player toPropel = Main.player.Where(n => n.active && !n.dead && n.Hitbox.Intersects(Projectile.Hitbox) && n.GetModPlayer<WindTunnelPlayer>().duration <= 0).FirstOrDefault();
			if (toPropel != default)
			{
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item45, toPropel.Center);
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item104, toPropel.Center);
				toPropel.GetModPlayer<WindTunnelPlayer>().oldVelocity = toPropel.velocity;
				toPropel.GetModPlayer<WindTunnelPlayer>().direction = Vector2.Normalize(toPropel.velocity);
				toPropel.GetModPlayer<WindTunnelPlayer>().duration = 15;
			}
		}
	}

	public class WindTunnelPlayer : ModPlayer
	{
		public Vector2 oldVelocity;

		public Vector2 direction;

		public int duration;

		public override void PostUpdate()
		{
			if (--duration > 0)
			{
				int speed = 30;
				float progress = duration > 7 ? 1 - (duration - 7) / 8f : 1;
				Player.velocity = Dash.SignedLesserBound(direction * speed * progress, Player.velocity * progress);
				Player.gravity = 0;
				Player.maxFallSpeed = 999;
			}
			if (duration == 0)
			{
				Player.velocity = oldVelocity;
			}
		}
	}
}
