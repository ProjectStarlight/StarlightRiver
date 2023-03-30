using StarlightRiver.Content.Buffs;
using System;

namespace StarlightRiver.Content.Items.Forest
{
	internal class FeralWolfMountBite : ModProjectile
	{
		public ref float maxTime => ref Projectile.ai[0];

		public float Progress => 1 - Projectile.timeLeft / maxTime;

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
			Projectile.width = 90;
			Projectile.height = 60;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			if (Projectile.timeLeft > (int)maxTime)
				Projectile.timeLeft = (int)maxTime;

			Player owner = Main.player[Projectile.owner];
			Projectile.Center = owner.MountedCenter + Vector2.UnitX.RotatedBy(Projectile.rotation) * 30 + Vector2.UnitY * 10;

			if (Projectile.timeLeft == (int)(maxTime / 2))
			{
				for (int k = 0; k < 50; k++)
				{
					Dust.NewDustPerfect(Projectile.Center, Terraria.ID.DustID.Blood, Vector2.UnitX.RotatedBy(Projectile.rotation).RotatedByRandom(1f) * Main.rand.NextFloat(5));
				}
			}

			Projectile.velocity = Vector2.UnitX.RotatedBy(Projectile.rotation) * 0.01f;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(ModContent.BuffType<WolfBleeding>(), 120);

			for (int k = 0; k < 50; k++)
			{
				Dust.NewDustPerfect(Projectile.Center, Terraria.ID.DustID.Blood, Vector2.UnitX.RotatedBy(Projectile.rotation).RotatedByRandom(1f) * Main.rand.NextFloat(15));
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D topTex = ModContent.Request<Texture2D>(AssetDirectory.ForestItem + "FeralWolfMountBiteTop").Value;
			Texture2D botTex = ModContent.Request<Texture2D>(AssetDirectory.ForestItem + "FeralWolfMountBiteBot").Value;
			Texture2D glowTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/TriTell").Value;

			float swoop = (float)Math.Log10(Progress * 100 + 1) * 0.65f;
			Color glowColor = new Color(255, 70, 60) * (float)Math.Sin((float)Math.Pow(Progress, 0.5f) * 3.14f);
			glowColor.A = 0;

			Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation + 1.57f / 2, new Vector2(0, glowTex.Height), 0.15f, 0, 0);

			int direction = Projectile.rotation == 0 ? 1 : -1;
			int direction2 = Projectile.rotation == 0 ? 0 : 1;

			float rotOff = direction * (3.14f + swoop * -2.54f);
			SpriteEffects effects = Projectile.rotation == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Main.EntitySpriteDraw(topTex, Projectile.Center - Main.screenPosition + Vector2.UnitY * -16, null, glowColor, -rotOff, new Vector2(topTex.Width * direction2, 0), 1, effects, 0);
			Main.EntitySpriteDraw(botTex, Projectile.Center - Main.screenPosition + Vector2.UnitY * 16, null, glowColor, rotOff, new Vector2(topTex.Width * direction2, botTex.Height), 1, effects, 0);

			return false;
		}
	}

	internal class WolfBleeding : SmartBuff
	{
		public override string Texture => AssetDirectory.Debug;

		public WolfBleeding() : base("Gored", "Gored by a wolf", true, false) { }

		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.lifeRegen -= 20;

			for (int k = 0; k < 2; k++)
			{
				Dust.NewDust(npc.position, npc.width, npc.height, Terraria.ID.DustID.Blood);
			}
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.lifeRegen -= 20;
			player.lifeRegenTime = 0;

			for (int k = 0; k < 2; k++)
			{
				Dust.NewDust(player.position, player.width, player.height, Terraria.ID.DustID.Blood);
			}
		}
	}
}