using StarlightRiver.Core.Systems.CameraSystem;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Lightsaber
{
	public class LightsaberProj_Green : LightsaberProj
	{
		protected override Vector3 BladeColor => Color.Green.ToVector3();

		bool jumped = false;

		private int soundTimer = 0;
		protected override void RightClickBehavior()
		{
			if (!jumped)
			{
				jumped = true;
				Owner.velocity = Owner.DirectionTo(Main.MouseWorld) * 20;
				Owner.GetModPlayer<LightsaberPlayer>().jumping = true;
			}

			if (soundTimer++ % 100 == 0)
			{
				hit = new List<NPC>();
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15 with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Owner.Center);
			}

			Owner.itemTime = Owner.itemAnimation = 2;
			Projectile.timeLeft = 200;
			afterImageLength = 30;
			Projectile.rotation += 0.06f * Owner.direction;
			rotVel = 0.02f;
			midRotation = Owner.velocity.ToRotation();
			squish = 0.7f;
			hide = false;
			canHit = true;
			anchorPoint = Owner.Center - Main.screenPosition;
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			Projectile.velocity = Vector2.Zero;
			Projectile.Center = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			Owner.heldProj = Projectile.whoAmI;

			if (!Owner.GetModPlayer<LightsaberPlayer>().jumping)
			{
				CameraSystem.shake += 10;
				for (int i = 0; i < 30; i++)
					Dust.NewDustPerfect(Owner.Bottom, ModContent.DustType<LightsaberGlow>(), Main.rand.NextVector2Circular(10, 10), 0, new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), Main.rand.NextFloat(1.95f, 2.35f));
				Projectile.active = false;
				Tile tile = Main.tile[(Owner.Bottom / 16 + new Vector2(0, 1)).ToPoint()];
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.UnitX, ModContent.ProjectileType<GreenLightsaberShockwave>(), (int)(Projectile.damage * 1.3f), 0, Owner.whoAmI, 0, 10);
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.UnitX * -1, ModContent.ProjectileType<GreenLightsaberShockwave>(), (int)(Projectile.damage * 1.3f), 0, Owner.whoAmI, tile.TileType, -10);
				var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Owner.Bottom, Vector2.Zero, ModContent.ProjectileType<LightsaberImpactRing>(), 0, 0, Owner.whoAmI, 160, 1.57f);
				(proj.ModProjectile as LightsaberImpactRing).outerColor = new Color(BladeColor.X, BladeColor.Y, BladeColor.Z);
				(proj.ModProjectile as LightsaberImpactRing).ringWidth = 40;
				(proj.ModProjectile as LightsaberImpactRing).timeLeftStart = 50;
				(proj.ModProjectile as LightsaberImpactRing).additive = true;
				proj.timeLeft = 50;
			}
		}
	}
}