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
			Projectile.timeLeft = 200;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.ArmorPenetration = 10;
		}

		public override void AI()
		{
			if (Projectile.timeLeft > (int)Projectile.ai[0])
				Projectile.timeLeft = (int)Projectile.ai[0];

			var owner = Main.player[Projectile.owner];
			Projectile.Center = owner.MountedCenter + Vector2.UnitX.RotatedBy(Projectile.rotation) * 50;

			if (Projectile.timeLeft == (int)(Projectile.ai[0] / 2))
			{
				for (int k = 0; k < 50; k++)
					Dust.NewDustPerfect(Projectile.Center, Terraria.ID.DustID.Blood, Vector2.UnitX.RotatedBy(Projectile.rotation).RotatedByRandom(1f) * Main.rand.NextFloat(5));
			}	
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return Helpers.Helper.CheckConicalCollision(Projectile.Center, 100, Projectile.rotation % 6.28f, 1.57f, targetHitbox);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(Terraria.ID.BuffID.Bleeding, 60);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			var topTex = ModContent.Request<Texture2D>(AssetDirectory.ForestItem + "FeralWolfMountBiteTop").Value;
			var botTex = ModContent.Request<Texture2D>(AssetDirectory.ForestItem + "FeralWolfMountBiteBot").Value;
			var glowTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/TriTell").Value;

			var swoop = (float)Math.Log10(Progress * 100 + 1) * 0.65f;
			var glowColor = new Color(255, 70, 60) * (float)Math.Sin((float)Math.Pow(Progress, 0.5f) * 3.14f);
			glowColor.A = 0;

			Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation + 1.57f / 2, new Vector2(0, glowTex.Height), 0.15f, 0, 0);

			var direction = Projectile.rotation == 0 ? 1 : -1;
			var direction2 = Projectile.rotation == 0 ? 0 : 1;

			float rotOff = direction * (3.14f + swoop * -2.54f);
			var effects = Projectile.rotation == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Main.EntitySpriteDraw(topTex, Projectile.Center - Main.screenPosition + Vector2.UnitY * -16, null, glowColor, -rotOff, new Vector2(topTex.Width * direction2, 0), 1, effects, 0);
			Main.EntitySpriteDraw(botTex, Projectile.Center - Main.screenPosition + Vector2.UnitY * 16, null, glowColor, rotOff, new Vector2(topTex.Width * direction2, botTex.Height), 1, effects, 0);

			return false;
		}
	}
}
