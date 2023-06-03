using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class DiceAmmo : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Dice");
			Tooltip.SetDefault("Every shot has randomized stats");
		}

		public override void SetDefaults()
		{
			Item.width = 8;
			Item.height = 16;
			Item.value = 1000;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(0, 0, 0, 40);

			Item.maxStack = 999;

			Item.damage = 15;
			Item.knockBack = 1.5f;
			Item.ammo = AmmoID.Bullet;

			Item.DamageType = DamageClass.Ranged;
			Item.consumable = true;

			Item.shoot = ModContent.ProjectileType<DiceProj>();
			Item.shootSpeed = 3f;
		}
	}

	public class DiceProj : ModProjectile
	{
		private enum Stat
		{
			none = 0,
			damage = 1,
			knockback = 2,
			velocity = 3,
			gravity = 4
		}

		private Stat primaryStat = Stat.none;
		private Stat secondaryStat = Stat.none;
		private Stat tertiaryStat = Stat.none;

		const int FRAME_COUNT = 6;

		private bool initialized = false;

		private float gravity = 0.05f;

		float damageMult = 1;
		float knockBackMult = 1;
		float gravityMult = 1;
		float velocityMult = 1;

		private Color glowColor = Color.White;

		private readonly List<Vector2> oldPos = new();
		private readonly List<float> oldRot = new();

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Dice");
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 600;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Projectile.extraUpdates = 1;
			Projectile.frame = Main.rand.Next(FRAME_COUNT);
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			if (!initialized)
			{
				initialized = true;

				primaryStat = (Stat)(Main.rand.Next(4) + 1);

				if (Main.rand.NextBool(15))
				{
					while (secondaryStat != primaryStat)
						secondaryStat = (Stat)(Main.rand.Next(4) + 1);
				}

				if (Main.rand.NextBool(205))
				{
					while (tertiaryStat != primaryStat && tertiaryStat != secondaryStat)
						tertiaryStat = (Stat)(Main.rand.Next(4) + 1);
				}

				ApplyStats(primaryStat);
				ApplyStats(secondaryStat);
				ApplyStats(tertiaryStat);

				Projectile.damage = (int)(Projectile.damage * damageMult);
				Projectile.knockBack *= knockBackMult;
				Projectile.velocity *= velocityMult;
				gravity *= gravityMult;
				glowColor.A = 0;
			}

			oldPos.Add(Projectile.Center);
			oldRot.Add(Projectile.rotation);

			while (oldPos.Count > 8)
			{
				oldPos.RemoveAt(0);
				oldRot.RemoveAt(0);
			}

			Projectile.velocity.X *= 0.995f;
			Projectile.velocity.Y += gravity;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			int width = tex.Width / FRAME_COUNT;
			var sourceRect = new Rectangle(Projectile.frame * width, 0, width, tex.Height);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, sourceRect, lightColor, Projectile.rotation, new Vector2(width, tex.Height) / 2, 1f, default, default);
			Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, sourceRect, glowColor, Projectile.rotation, new Vector2(width, tex.Height) / 2, 1f, default, default);

			if (oldPos.Count == 0)
				return false;

			for (int i = 0; i < oldPos.Count; i++)
			{
				Vector2 pos = oldPos[i];
				float rot = oldRot[i];
				float opacity = i / (float)oldPos.Count;
				Main.spriteBatch.Draw(glowTex, pos - Main.screenPosition, sourceRect, glowColor * opacity, rot, new Vector2(width, tex.Height) / 2, 1f * opacity, default, default);
			}

			return false;
		}

		private void ApplyStats(Stat stat)
		{
			switch (stat)
			{
				case Stat.damage:
					damageMult *= Main.rand.NextFloat(2f, 3f);
					velocityMult *= Main.rand.NextFloat(0.75f, 0.95f);
					knockBackMult *= Main.rand.NextFloat(0.5f, 0.75f);
					gravityMult *= Main.rand.NextFloat(0.5f, 0.75f);
					glowColor = Color.Multiply(Color.Red, 1);
					break;

				case Stat.velocity:
					velocityMult *= Main.rand.NextFloat(1.25f, 1.6f);
					damageMult *= Main.rand.NextFloat(0.5f, 0.75f);
					knockBackMult *= Main.rand.NextFloat(0.5f, 0.75f);
					gravityMult *= Main.rand.NextFloat(0.5f, 0.75f);
					glowColor = Color.Multiply(Color.Blue, 1);
					break;

				case Stat.knockback:
					knockBackMult *= Main.rand.NextFloat(2f, 3f);
					damageMult *= Main.rand.NextFloat(0.5f, 0.75f);
					velocityMult *= Main.rand.NextFloat(0.75f, 0.95f);
					gravityMult *= Main.rand.NextFloat(0.5f, 0.75f);
					glowColor = Color.Multiply(Color.Yellow, 1);
					break;

				case Stat.gravity:
					gravityMult *= Main.rand.NextFloat(2f, 3f);
					damageMult *= Main.rand.NextFloat(0.5f, 0.75f);
					velocityMult *= Main.rand.NextFloat(0.75f, 0.95f);
					knockBackMult *= Main.rand.NextFloat(0.5f, 0.75f);
					glowColor = Color.Multiply(Color.Green, 1);
					break;
			}
		}
	}
}