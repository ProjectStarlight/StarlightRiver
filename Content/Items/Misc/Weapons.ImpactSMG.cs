using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.Misc
{
	public class ImpactSMG : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public int ammo = 10;

		public float damageBoost = 1;

		private int reloadTimer = 0;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Impact SMG");
			Tooltip.SetDefault("Throw the gun when out of ammo \nHitting an enemy with the gun instantly reloads it");

		}

		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 6;
			Item.useAnimation = 6;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 0;
			Item.rare = ItemRarityID.Blue;
			Item.shoot = ModContent.ProjectileType<ImpactSMGProj>();
			Item.shootSpeed = 12f;
			Item.useAmmo = AmmoID.Bullet;
			Item.autoReuse = true;
		}

        public override void HoldItem(Player player)
        {
			if (ammo == -1)
				reloadTimer++;
			else
				reloadTimer = 0;

			if (reloadTimer > 120)
            {
				ammo = 15;
				damageBoost = 1f;
				reloadTimer = 0;
            }
        }

        public override bool CanUseItem(Player Player)
		{
			if (ammo == -1)
				return false;
			if (ammo <= 1)
			{
				Item.useTime = 30;
				Item.useAnimation = 30;
				if (ammo <= 0)
				{
					Item.noUseGraphic = true;
					Item.useStyle = ItemUseStyleID.Swing;
				}
			}
			else
			{
				Item.noUseGraphic = false;
				Item.useStyle = ItemUseStyleID.Shoot;
				Item.useTime = 6;
				Item.useAnimation = 6;
			}
			return base.CanUseItem(Player);
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			if (ammo > 0)
				velocity = velocity.RotatedByRandom(0.3f);
		}
		public override Vector2? HoldoutOffset()
		{
			return new Vector2(0, 0);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (ammo <= 0)
			{
				if (ammo == 0)
				{
					Projectile.NewProjectile(source, position, velocity * 0.75f, Item.shoot, (int)(damage * damageBoost) * 2, knockback, player.whoAmI);
					ammo--;
				}
				return false;
			}
			ammo--;
			float rot = velocity.ToRotation();
			float spread = 0.4f;

			Vector2 offset = new Vector2(1, -0.12f * player.direction).RotatedBy(rot);

			for (int k = 0; k < 5; k++)
			{
				var direction = offset.RotatedByRandom(spread);

				Dust.NewDustPerfect(position + (offset * 40), ModContent.DustType<Dusts.Glow>(), direction * Main.rand.NextFloat(8), 125, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.5f));
			}

			Helper.PlayPitched("Guns/RifleLight", 0.7f, Main.rand.NextFloat(-0.1f, 0.1f), position);
			Dust.NewDustPerfect(player.Center + offset * 40, ModContent.DustType<Dusts.Smoke>(), Vector2.UnitY * -2 + offset.RotatedByRandom(spread) * 5, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));

			Projectile proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), position, velocity.RotatedByRandom(spread) * 2, type, (int)(damage * damageBoost), knockback, player.whoAmI);
			proj.GetGlobalProjectile<CoachGunGlobalProj>().shotFromGun = true;

			Projectile.NewProjectile(player.GetSource_ItemUse(Item), position + (offset * 39), Vector2.Zero, ModContent.ProjectileType<CoachGunMuzzleFlash>(), 0, 0, player.whoAmI, rot);

			Gore.NewGore(source, player.Center + (offset * 10), new Vector2(player.direction * -1, -0.5f) * 2, Mod.Find<ModGore>("CoachGunCasing").Type, 1f);

			return false;
		}
	}
	public class ImpactSMGProj : ModProjectile
	{
		public override string Texture => AssetDirectory.MiscItem +"ImpactSMG";

		private List<Vector2> cache;
		private Trail trail;

		private Player owner => Main.player[Projectile.owner];
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Impact SMG");
		}

		public override void SetDefaults()
		{
			Projectile.width = 24;
			Projectile.height = 24;

			Projectile.aiStyle = 2;

			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
		}

		public override void AI()
		{
			ManageCaches();
			ManageTrail();
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (owner.HeldItem.type == ModContent.ItemType<ImpactSMG>())
            {
				owner.itemTime = owner.itemAnimation = 0;
				var mp = owner.HeldItem.ModItem as ImpactSMG;
				mp.ammo = 15;
				if (mp.damageBoost < 1.25f)
					mp.damageBoost += 0.05f;
			}
        }

		public override bool PreDraw(ref Color lightColor)
		{
			DrawTrail(Main.spriteBatch);
			return true;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 10; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 10)
			{
				cache.RemoveAt(0);
			}

		}

		private void ManageTrail()
		{

			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(4), factor => 10, factor =>
			{
				return Color.Gray;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		private void DrawTrail(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			Effect effect = Filters.Scene["CoachBombTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/MotionTrail").Value);

			trail?.Render(effect);

			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}
	}
}