using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StarlightRiver.Core;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Permafrost
{
	class AuroraBell : ModItem
	{
        public override string Texture => AssetDirectory.PermafrostItem + "AuroraBell";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Aurora Bell");
			Tooltip.SetDefault("Summons a sentry bell /nHit the bell with a whip to ring it");
		}

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.QueenSpiderStaff);
			Item.damage = 19;
			Item.mana = 12;
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(0, 0, 80, 0);
			Item.rare = ItemRarityID.Green;
			Item.knockBack = 2.5f;
			Item.UseSound = SoundID.Item25;
			Item.shoot = ModContent.ProjectileType<AuroraBellProj>();
			Item.shootSpeed = 0f;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Projectile proj = Projectile.NewProjectileDirect(source, Main.MouseWorld, velocity, type, damage, knockback, player.whoAmI);
			proj.originalDamage = Item.damage;
			player.UpdateMaxTurrets();
			return false;
		}
	}
	public class AuroraBellProj : ModProjectile
	{
		public override string Texture => AssetDirectory.PermafrostItem + "AuroraBellProj";

		private Player owner => Main.player[Projectile.owner];

		private int chargeCounter = 600;

		private float startRotation = 0f;

		private int ringDirection = 1;

		private int counter = 0;

		private float chargeRatio => chargeCounter / 600f;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Aurora Bell");
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 28;
			Projectile.timeLeft = Projectile.SentryLifeTime;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.sentry = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.ignoreWater = true;
		}

        public override bool PreDraw(ref Color lightColor)
        {
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			Vector2 offset = 8 * new Vector2((float)Math.Cos(counter * 0.05f), (float)Math.Sin(counter * 0.05f));

			float pulseProgress = chargeCounter / 40f;
			float transparency = (float)Math.Pow(MathHelper.Clamp(1 - pulseProgress, 0, 1), 2);
			float scale = (float)MathHelper.Clamp(1 + pulseProgress, 0, 2);

			Main.spriteBatch.Draw(tex, (Projectile.Center + offset - new Vector2(0, tex.Height / 2)) - Main.screenPosition, null, lightColor * transparency, Projectile.rotation, new Vector2(tex.Width / 2, 0), Projectile.scale * scale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(tex, (Projectile.Center + offset - new Vector2(0, tex.Height / 2)) - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(tex.Width / 2, 0), Projectile.scale, SpriteEffects.None, 0f);
			return false;
        }

        public override void AI()
        {
			Projectile.rotation = (float)(Math.Sin((Math.Pow(chargeCounter * 0.15f, 0.7f) * ringDirection) + startRotation)) * (float)Math.Pow(1 - chargeRatio, 3f);

			counter++;
			if (chargeCounter < 600)
				chargeCounter++;
			if (chargeCounter < 20)
				return;
			for (int i = 0; i < Main.projectile.Length; i++)
            {
				Projectile proj = Main.projectile[i];
				if (proj == null || !proj.active || !ProjectileID.Sets.IsAWhip[proj.type])
					continue;

				ModProjectile modProj = proj.ModProjectile;

				bool colliding = false;
				if (modProj != null && modProj.Colliding(proj.Hitbox, Projectile.Hitbox) == true)
				{
					if (modProj.Colliding(proj.Hitbox, Projectile.Hitbox) == null && Projectile.Colliding(proj.Hitbox, Projectile.Hitbox))
						colliding = true;
				}

				else
                {
					for (int n = 0; n < proj.WhipPointsForCollision.Count; n++)
					{
						Point point = proj.WhipPointsForCollision[n].ToPoint();
						Rectangle myRect = new Rectangle(0,0, proj.width, proj.height);
						myRect.Location = new Point(point.X - myRect.Width / 2, point.Y - myRect.Height / 2);
						if (myRect.Intersects(Projectile.Hitbox))
						{
							startRotation = (float)Math.Asin(Projectile.rotation);
							ringDirection = Math.Sign(owner.Center.X - Projectile.Center.X);
							colliding = true;
							break;
						}
					}
				}

				if (colliding)
				{
					Main.NewText("Collision");
					chargeCounter = 0;
					break;
				}
            }
        }
    }
}
