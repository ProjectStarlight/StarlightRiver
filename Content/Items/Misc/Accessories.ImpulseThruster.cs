using StarlightRiver.Content.Items.BaseTypes;
using System;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class ImpulseThruster : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public ImpulseThruster() : base("Impulse Thruster", "Converts all wingtime into a burst of energy") { }

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.Pink;
		}

		public override void SafeUpdateEquip(Player Player)
		{
			if (Player.controlJump && Player.wingTime > 0)
			{
				var dir = new Vector2(0, -1);

				if (Player.controlDown)
					dir.Y = 1;
				if (Player.controlUp)
					dir.Y = -1;
				if (Player.controlLeft)
					dir.X = -1;
				if (Player.controlRight)
					dir.X = 1;

				if (!Player.releaseJump)
				{
					for (int i = 0; i < 15; i++)
					{
						var dust = Dust.NewDustDirect(Player.Center - new Vector2(8, 8), 0, 0, ModContent.DustType<ImpulseThrusterDustOne>());
						dust.velocity = -dir.RotatedByRandom(0.4f) * Main.rand.NextFloat(20);
						dust.scale = Main.rand.NextFloat(1.2f, 1.9f);
						dust.alpha = Main.rand.Next(70);
						dust.rotation = Main.rand.NextFloat(6.28f);
					}

					for (int i = 0; i < 5; i++)
					{
						Vector2 projDir = dir.RotatedByRandom(1.4f);
						Projectile.NewProjectileDirect(Player.GetSource_Accessory(Item), Player.Center - projDir * 20, projDir * Main.rand.NextFloat(2, 3), ModContent.ProjectileType<ThrusterEmber>(), 0, 0, Player.whoAmI).scale = Main.rand.NextFloat(0.85f, 1.15f);
					}
				}

				dir.Normalize();

				float mult = MathHelper.Clamp((float)Math.Pow(Player.wingTime, 0.7f) * 3, 1, 70);
				dir *= mult;
				Player.velocity = dir;
				Player.wingTime = 0;
			}
		}
	}

	public class ThrusterEmber : ModProjectile
	{
		public override string Texture => AssetDirectory.Assets + "Invisible";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Thruster");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.aiStyle = 1;
			Projectile.width = Projectile.height = 12;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Projectile.extraUpdates = 1;
			Projectile.alpha = 255;
		}

		public override void AI()
		{
			Projectile.scale *= 0.98f;

			if (Main.rand.NextBool(2))
			{
				var dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<ImpulseThrusterDustTwo>(), Main.rand.NextVector2Circular(1.5f, 1.5f));
				dust.scale = 0.6f * Projectile.scale;
				dust.rotation = Main.rand.NextFloatDirection();
			}
		}
	}

	public class ImpulseThrusterDustOne : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "NeedlerDust";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.scale *= Main.rand.NextFloat(0.8f, 2f);
			dust.frame = new Rectangle(0, 0, 34, 36);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			var gray = new Color(25, 25, 25);
			Color ret;
			if (dust.alpha < 60)
				ret = Color.Lerp(Color.Yellow, Color.Orange, dust.alpha / 60f);
			else if (dust.alpha < 140)
				ret = Color.Lerp(Color.Orange, Color.OrangeRed, (dust.alpha - 60) / 80f);
			else if (dust.alpha < 200)
				ret = Color.Lerp(Color.OrangeRed, gray, (dust.alpha - 140) / 80f);
			else
				ret = gray;

			return ret * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			if (dust.velocity.Length() > 6)
				dust.velocity *= 0.92f;
			else
				dust.velocity *= 0.96f;

			if (dust.alpha > 100)
			{
				dust.scale += 0.01f;
				dust.alpha += 2;
			}
			else
			{
				Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.1f);
				dust.scale *= 0.985f;
				dust.alpha += 4;
			}

			dust.position += dust.velocity;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}

	public class ImpulseThrusterDustTwo : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "NeedlerDust";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.scale *= Main.rand.NextFloat(0.8f, 2f);
			dust.frame = new Rectangle(0, 0, 34, 36);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			var gray = new Color(25, 25, 25);
			Color ret;
			if (dust.alpha < 40)
				ret = Color.Lerp(Color.Yellow, Color.OrangeRed, dust.alpha / 40f);
			else if (dust.alpha < 80)
				ret = Color.Lerp(Color.OrangeRed, gray, (dust.alpha - 40) / 40f);
			else
				ret = gray;

			return ret * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			if (dust.velocity.Length() > 3)
				dust.velocity *= 0.85f;
			else
				dust.velocity *= 0.92f;

			if (dust.alpha > 60)
			{
				dust.scale += 0.01f;
				dust.alpha += 6;
			}
			else
			{
				Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.1f);
				dust.scale *= 0.985f;
				dust.alpha += 4;
			}

			dust.position += dust.velocity;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}
}