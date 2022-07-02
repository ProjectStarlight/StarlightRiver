using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Core.Systems.CombatMountSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Forest
{
	internal class FeralWolfMountBite : ModProjectile
	{
		public float Progress => 1 - (Projectile.timeLeft / Projectile.ai[0]);

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crushing Jaws");
		}

		public override void SetDefaults()
		{
			Projectile.timeLeft = (int)Projectile.ai[0];
			Projectile.DamageType = DamageClass.Summon;
			Projectile.friendly = true;
		}

		public override void AI()
		{
			var owner = Main.player[Projectile.owner];
			Projectile.Center = owner.MountedCenter + Vector2.UnitX.RotatedBy(Projectile.rotation) * 50;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return Helpers.Helper.CheckConicalCollision(Projectile.Center, 50, Projectile.rotation, 1.57f, targetHitbox);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			var topTex = ModContent.Request<Texture2D>(AssetDirectory.ForestItem + "FeralWolfMountBiteTop").Value;
			var botTex = ModContent.Request<Texture2D>(AssetDirectory.ForestItem + "FeralWolfMountBiteBot").Value;
			var glowTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/TriTell").Value;

			var glowColor = new Color(255, 100, 100) * (float)Math.Sin(Progress * 3.14f);
			glowColor.A = 0;

			Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation + 1.57f / 2, new Vector2(0, glowTex.Height), 1, 0, 0);

			float rotOff = 0.5f - Helpers.Helper.SwoopEase(Progress) * 0.5f;

			Main.EntitySpriteDraw(topTex, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation + 1.57f / 2 - rotOff, Vector2.Zero, 1, 0, 0);
			Main.EntitySpriteDraw(botTex, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation + 1.57f / 2 + rotOff, new Vector2(0, botTex.Height), 1, 0, 0);

			return false;
		}
	}
}
