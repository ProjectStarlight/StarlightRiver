﻿using Microsoft.Xna.Framework;
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
	public class LightsaberProj_White : LightsaberProj
	{
		protected override Vector3 BladeColor => new Color(200, 200, 255).ToVector3();

		private bool spawnedSecond = false;

        protected override void RightClickBehavior()
        {
			owner.GetModPlayer<LightsaberPlayer>().whiteCooldown = 1200;
			if (nonEasedProgress > 0.5f && !spawnedSecond)
			{
				Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), owner.Center, Vector2.Zero, ModContent.ProjectileType<LightsaberProj_White>(), Projectile.damage, Projectile.knockBack, owner.whoAmI);
				(proj.ModProjectile as LightsaberProj_White).frontHand = false;
				(proj.ModProjectile as LightsaberProj_White).spawnedSecond = true;
				(proj.ModProjectile as LightsaberProj_White).rightClicked = true;
				spawnedSecond = true;
			}
			hide = false;
			canHit = true;
			if (thrown)
				ThrownBehavior();
			else
				HeldBehavior();

			if (Projectile.ai[0] >= 1 && Main.mouseRight)
				Projectile.ai[0] = 0;
		}
    }
}