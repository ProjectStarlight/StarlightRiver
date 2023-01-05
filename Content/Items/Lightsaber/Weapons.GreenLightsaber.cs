using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.GameContent;

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
				owner.velocity = owner.DirectionTo(Main.MouseWorld) * 20;
				owner.GetModPlayer<LightsaberPlayer>().jumping = true;
			}

			if (soundTimer++ % 100 == 0)
			{
				hit = new List<NPC>();
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15 with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, owner.Center);
			}
			owner.itemTime = owner.itemAnimation = 2;
			Projectile.timeLeft = 200;
			afterImageLength = 30;
			Projectile.rotation += 0.06f * owner.direction;
			rotVel = 0.02f;
			midRotation = owner.velocity.ToRotation();
			squish = 0.7f;
			hide = false;
			canHit = true;
			anchorPoint = owner.Center - Main.screenPosition;
			owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			Projectile.velocity = Vector2.Zero;
			Projectile.Center = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			owner.heldProj = Projectile.whoAmI;

			if (!owner.GetModPlayer<LightsaberPlayer>().jumping)
			{
				Core.Systems.CameraSystem.Shake += 10;
				for (int i = 0; i < 30; i++)
					Dust.NewDustPerfect(owner.Bottom, ModContent.DustType<LightsaberGlow>(), Main.rand.NextVector2Circular(10, 10), 0, new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), Main.rand.NextFloat(1.95f, 2.35f));
				Projectile.active = false;
				Tile tile = Main.tile[((owner.Bottom / 16) + new Vector2(0, 1)).ToPoint()];
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.UnitX, ModContent.ProjectileType<Lightsaber_GreenShockwave>(), (int)(Projectile.damage * 1.3f), 0, owner.whoAmI, 0, 10);
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.UnitX * -1, ModContent.ProjectileType<Lightsaber_GreenShockwave>(), (int)(Projectile.damage * 1.3f), 0, owner.whoAmI, tile.TileType, -10);
				Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), owner.Bottom, Vector2.Zero, ModContent.ProjectileType<LightsaberImpactRing>(), 0, 0, owner.whoAmI, 160, 1.57f);
				(proj.ModProjectile as LightsaberImpactRing).outerColor = new Color(BladeColor.X, BladeColor.Y, BladeColor.Z);
				(proj.ModProjectile as LightsaberImpactRing).ringWidth = 40;
				(proj.ModProjectile as LightsaberImpactRing).timeLeftStart = 50;
				(proj.ModProjectile as LightsaberImpactRing).additive = true;
				proj.timeLeft = 50;
			}
		}
	}
}