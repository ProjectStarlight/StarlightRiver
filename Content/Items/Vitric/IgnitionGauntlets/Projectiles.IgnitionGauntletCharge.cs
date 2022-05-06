using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.Vitric
{
	public class IgnitionGauntletCharge : ModProjectile
	{
		int charge = 0;
		public override string Texture => AssetDirectory.Assets + "Invisible";
		private Player owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.timeLeft = 9999;
			Projectile.width = Projectile.height = 50;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Projectile.hide = true;
		}

		public override void AI()
		{
			IgnitionPlayer modPlayer = owner.GetModPlayer<IgnitionPlayer>();
			Projectile.Center = owner.Center;
			if (Main.mouseRight)
			{
				if (modPlayer.charge > charge)
				{
					charge += 3;
				}
				modPlayer.potentialCharge = charge;
				Main.NewText(charge.ToString(), 255, 0, 100);
			}
			else
			{
				modPlayer.potentialCharge = 0;
				if (!owner.GetModPlayer<IgnitionPlayer>().launching)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), owner.Center, Vector2.Zero, ModContent.ProjectileType<IgnitionGauntletLaunch>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
					//owner.velocity = owner.DirectionTo(Main.MouseWorld) * 20;
				}

				//UNCOMMENT THIS BEFORE RELEASE
				//modPlayer.charge -= charge;
				modPlayer.loadedCharge = charge;
				owner.GetModPlayer<IgnitionPlayer>().launching = true;
				Projectile.active = false;
			}
		}
	}
}