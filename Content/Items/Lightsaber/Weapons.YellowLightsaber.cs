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
	public class LightsaberProj_Yellow : LightsaberProj
	{
		protected override Vector3 BladeColor => Color.Yellow.ToVector3() * 0.8f * fade;

		private bool dashing = false;

		private bool caughtUp = false;

		private float fade = 1f;

        protected override void RightClickBehavior()
        {
			hide = true;
			canHit = false;
			Projectile.active = false;
        }

        protected override void SafeLeftClickBehavior()
        {
			if (!thrown)
				return;

			if (Main.mouseRight && !dashing)
			{
				dashing = true;
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<LightsaberProj_YellowDash>(), Projectile.damage * 2, 0, owner.whoAmI);
				owner.GetModPlayer<LightsaberPlayer>().dashing = true;
			}

			if (dashing)
            {
				Projectile.velocity = Vector2.Zero;
				if (owner.Distance(Projectile.Center) < 80 || !owner.GetModPlayer<LightsaberPlayer>().dashing && !caughtUp)
                {
					owner.Center = Projectile.Center;
					owner.velocity = Vector2.Zero;
					owner.GetModPlayer<LightsaberPlayer>().dashing = false;
					Projectile.active = true;
					caughtUp = true;
				}

				if (caughtUp)
				{
					Projectile.active = true;
					fade -= 0.01f;
					if (fade <= 0)
						Projectile.active = false;
				}
				else
				{
					owner.velocity = owner.DirectionTo(Projectile.Center) * 60;
					Dust dust = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(45, 45) + owner.velocity, ModContent.DustType<Dusts.GlowLine>(), owner.DirectionTo(Projectile.Center) * Main.rand.NextFloat(2), 0, new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), Main.rand.NextFloat(1f, 1.5f));
					dust.fadeIn = 20;

					Dust.NewDustPerfect(owner.Center + owner.velocity + Main.rand.NextVector2Circular(30, 30), ModContent.DustType<LightsaberGlow>(), Vector2.Normalize(owner.velocity).RotatedBy(Main.rand.NextFloat(2.5f, 3f)) * Main.rand.NextFloat(3), 0, new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), Main.rand.NextFloat(0.5f, 0.85f));
					Dust.NewDustPerfect(owner.Center + owner.velocity + Main.rand.NextVector2Circular(30, 30), ModContent.DustType<LightsaberGlow>(), Vector2.Normalize(owner.velocity).RotatedBy(-Main.rand.NextFloat(2.5f, 3f)) * Main.rand.NextFloat(3), 0, new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), Main.rand.NextFloat(0.5f, 0.85f));
				}
			}
        }
    }
}