using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;

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
				if (proj == null || !proj.active || !(ProjectileID.Sets.IsAWhip[proj.type] || (proj.type == ModContent.ProjectileType<AuroraBellRing>())))
					continue;

				ModProjectile modProj = proj.ModProjectile;
                if (modProj is AuroraBellRing ringer && ringer.cantHit.Contains(Projectile))
                    continue;

				bool colliding = false;
				if (modProj != null && modProj.Colliding(proj.Hitbox, Projectile.Hitbox) != false)
				{
					if (modProj.Colliding(proj.Hitbox, Projectile.Hitbox) == null && Projectile.Colliding(proj.Hitbox, Projectile.Hitbox))
						colliding = true;
                    if (modProj.Colliding(proj.Hitbox, Projectile.Hitbox) == true)
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
                    Vector2 offset = 8 * new Vector2((float)Math.Cos(counter * 0.05f), (float)Math.Sin(counter * 0.05f));
                    Projectile newProj = Projectile.NewProjectileDirect(Projectile.GetProjectileSource_FromThis(), Projectile.Center + offset, Vector2.Zero, ModContent.ProjectileType<AuroraBellRing>(), Projectile.damage, Projectile.knockBack, owner.whoAmI);
					newProj.originalDamage = Projectile.damage;

                    if (modProj is AuroraBellRing oldRinger)
                    {
                        var newProjMP = newProj.ModProjectile as AuroraBellRing;
                        oldRinger.cantHit.Add(Projectile);
                        newProjMP.cantHit = oldRinger.cantHit;
                    }
					chargeCounter = 0;
					break;
				}
            }
        }
    }
    internal class AuroraBellRing : ModProjectile, IDrawPrimitive
    {
        public override string Texture => AssetDirectory.Assets + "Invisible";

        private List<Vector2> cache;

        private Trail trail;
        private Trail trail2;

        private float Progress => 1 - (Projectile.timeLeft / 16f);

        private float Radius => 180 * (float)Math.Sqrt(Math.Sqrt(Progress));

        public List<Projectile> cantHit = new List<Projectile>();

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 16;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Aurora Bell");
        }

        public override void AI()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                ManageCaches();
                ManageTrail();
            }
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public override bool? CanHitNPC(NPC target)
        {
            if (target.whoAmI == (int)Projectile.ai[0])
                return false;
            return base.CanHitNPC(target);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
            line.Normalize();
            line *= Radius;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line))
            {
                return true;
            }
            return false;
        }

        private void ManageCaches()
        {
            cache = new List<Vector2>();
            float radius = Radius;
            for (int i = 0; i < 33; i++) //TODO: Cache offsets, to improve performance
            {
                double rad = (i / 32f) * 6.28f;
                Vector2 offset = new Vector2((float)Math.Sin(rad), (float)Math.Cos(rad));
                offset *= radius;
                cache.Add(Projectile.Center + offset);
            }

            while (cache.Count > 33)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {

            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 38 * (1 - Progress), factor =>
            {
                return Color.Magenta;
            });

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 20 * (1 - Progress), factor =>
            {
                return Color.White;
            });
            float nextplace = 33f / 32f;
            Vector2 offset = new Vector2((float)Math.Sin(nextplace), (float)Math.Cos(nextplace));
            offset *= Radius;

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + offset;

            trail2.Positions = cache.ToArray();
            trail2.NextPosition = Projectile.Center + offset;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
            effect.Parameters["alpha"].SetValue(1);

            trail?.Render(effect);
            trail2?.Render(effect);
        }
    }
}
