using System;
using Terraria.Audio;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
	class MonkSpade : ModItem
	{
		public float bonusChance = 0;

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.Melee;
			Item.width = 32;
			Item.height = 32;
			Item.damage = 20;
			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.knockBack = 2.5f;
			Item.autoReuse = true;

			Item.UseSound = new SoundStyle($"{nameof(StarlightRiver)}/Sounds/GlassMinibossSword");

			Item.shoot = ProjectileType<MonkSpadeProjectile>();
			Item.shootSpeed = 1;
		}
	}

	class MonkSpadeProjectile : ModProjectile
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetDefaults()
		{
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.timeLeft = 40;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			Player Player = Main.player[Projectile.owner];

			Vector2 center = Player.Center + Vector2.UnitY * Player.gfxOffY;

			if (Projectile.timeLeft == 40)
				Projectile.rotation = (Player.Center - Main.MouseWorld).ToRotation();

			Player.heldProj = Projectile.whoAmI;

			Projectile.extraUpdates = Projectile.timeLeft > 15 ? 2 : 0;

			Projectile.Center = center + Vector2.UnitX.RotatedBy(Projectile.rotation + (Projectile.timeLeft - 20f) / 20f * 0.3f * -Player.direction) * (float)Math.Sin(Projectile.timeLeft / 40f * Math.PI) * -100;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player Player = Main.player[Projectile.owner];

			float fade =
				Projectile.timeLeft > 35 ? (40 - Projectile.timeLeft) / 5f :
				Projectile.timeLeft < 5 ? Projectile.timeLeft / 5f :
				1;

			Texture2D tex = Request<Texture2D>(Texture).Value;
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor * fade, (Projectile.Center - Player.Center).ToRotation() + (float)Math.PI - (float)Math.PI / 4, new Vector2(8, 8), 1, 0, 0);

			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			var ownerSpade = Main.player[Projectile.owner].HeldItem.ModItem as MonkSpade;

			if (Projectile.type == ProjectileType<MonkSpadeProjectile>() && ownerSpade != null)
			{
				if (target.life <= 0)
				{
					if (Main.rand.NextFloat() < ownerSpade.bonusChance)
					{
						Item.NewItem(Projectile.GetSource_OnHit(target), target.Center, ItemID.Heart);
						ownerSpade.bonusChance = 0;
					}
					else
					{
						ownerSpade.bonusChance += 0.1f;
					}
				}
				else
				{
					ownerSpade.bonusChance += 0.05f;
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.friendly = false; //cant deal damage through walls
			return false;
		}
	}
}
