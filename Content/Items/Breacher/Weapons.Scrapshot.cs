using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Bosses.VitricBoss;
using Terraria.Graphics.Effects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Breacher
{
	public class Scrapshot : ModItem
	{
		public ScrapshotHook hook;

		public override string Texture => AssetDirectory.BreacherItem + Name;

		public override bool AltFunctionUse(Player player) => true;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Scrapshot");
			Tooltip.SetDefault("Right click to fire a chain hook\nShooting while hooked has reduced spread");
		}

		public override void SetDefaults()
		{
			item.width = 24;
			item.height = 28;
			item.damage = 7;
			item.useAnimation = 30;
			item.useTime = 30;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.knockBack = 2f;
			item.rare = ItemRarityID.Orange;
			item.value = Item.sellPrice(0, 10, 0, 0);
			item.noMelee = true;
			item.useTurn = false;
			item.useAmmo = AmmoID.Bullet;
			item.ranged = true;
			item.shoot = 1;
			item.shootSpeed = 17;
		}

		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				item.useTime = 2;
				item.useAnimation = 2;
				item.noUseGraphic = true;

				return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<ScrapshotHook>());
			}
			else
			{
				item.useTime = 30;
				item.useAnimation = 30;
				item.noUseGraphic = false;

				return true;
			}
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			if (player.altFunctionUse == 2)
			{
				int i = Projectile.NewProjectile(player.Center, new Vector2(speedX, speedY), ModContent.ProjectileType<ScrapshotHook>(), damage, knockBack, player.whoAmI);
				hook = Main.projectile[i].modProjectile as ScrapshotHook;

				Helper.PlayPitched("Guns/ChainShoot", 1, 0, player.Center);
			}
			else if (hook is null || (hook != null && (!hook.projectile.active || hook.projectile.type != ModContent.ProjectileType<ScrapshotHook>() || hook.hooked != null)))
			{
				Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 8;

				float spread = 0.5f;

				if (type == ProjectileID.Bullet)
					type = ModContent.ProjectileType<ScrapshotShrapnel>();

				if (hook != null && hook.projectile.type == ModContent.ProjectileType<ScrapshotHook>() && hook.projectile.active && hook.hooked != null)
				{
					spread = 0.05f;
					damage += 10;

					hook.struck = true;
					hook.projectile.timeLeft = 20;

					player.velocity = Vector2.Normalize(hook.startPos - Main.MouseWorld) * 12;
					player.GetModPlayer<StarlightPlayer>().Shake += 12;

					Helper.PlayPitched("ChainHit", 1, 0, player.Center);

					for (int k = 0; k < 20; k++)
					{
						var direction = Vector2.One.RotatedByRandom(6.28f);
						Dust.NewDustPerfect(player.Center + direction * 10, ModContent.DustType<Dusts.Glow>(), direction * Main.rand.NextFloat(2, 4), 0, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.5f));
					}
				}

				float rot = new Vector2(speedX, speedY).ToRotation();

				for (int k = 0; k < 6; k++)
				{
					var direction = Vector2.UnitX.RotatedBy(rot).RotatedByRandom(spread);

					int i = Projectile.NewProjectile(player.Center, direction * item.shootSpeed, type, damage, knockBack, player.whoAmI);
					Main.projectile[i].timeLeft = 30;

					Dust.NewDustPerfect(player.Center + direction * 60, ModContent.DustType<Dusts.Glow>(), direction * Main.rand.NextFloat(20), 0, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.5f));
					Dust.NewDustPerfect(player.Center + direction * 60, ModContent.DustType<Dusts.Smoke>(), Vector2.UnitY * -2 + direction * 5, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));
				}

				Helper.PlayPitched("Guns/Scrapshot", 1, 0, player.Center);
			}

			return false;
		}
	}

	public class ScrapshotHook : ModProjectile
	{
		public NPC hooked;
		public Vector2 startPos;
		public bool struck;

		public int timer;

		ref float Progress => ref projectile.ai[0];
		ref float Distance => ref projectile.ai[1];
		bool Retracting => projectile.timeLeft < 30;

		public override string Texture => AssetDirectory.BreacherItem + Name;

		public override void SetDefaults()
		{
			projectile.width = 16;
			projectile.height = 16;
			projectile.friendly = true;
			projectile.timeLeft = 60;
			projectile.aiStyle = -1;
			projectile.penetrate = 2;
		}

		public override void AI()
		{
			Player player = Main.player[projectile.owner];

			projectile.rotation = projectile.velocity.ToRotation();
			if (projectile.timeLeft < 40)//slows down the projectile by 8%, for about 10 ticks before it retracts
				projectile.velocity *= 0.92f;

			if (projectile.timeLeft == 30)
			{
				startPos = projectile.Center;
				projectile.velocity *= 0;
			}

			if (Retracting)
				projectile.Center = Vector2.Lerp(player.Center, startPos, projectile.timeLeft / 30f);

			if (hooked != null && !struck)
			{
				timer++;

				player.direction = startPos.X > hooked.Center.X ? -1 : 1;

				if (timer == 1)
					Helper.PlayPitched("Guns/ChainPull", 1, 0, player.Center);

				if (timer < 10)
				{
					player.velocity *= 0.96f;
					return;
				}

				if (timer == 10)
					startPos = player.Center;

				projectile.timeLeft = 52;
				projectile.Center = hooked.Center;
				player.velocity = Vector2.Zero;//resets wings / double jumps

				Progress += (10f / Distance) * (0.8f + Progress * 1.5f);
				player.Center = Vector2.Lerp(startPos, hooked.Center, Progress);
				player.Center = player.Center + Vector2.UnitY * player.velocity.Y;

				if (player.Hitbox.Intersects(hooked.Hitbox))
				{
					struck = true;
					projectile.timeLeft = 20;

					player.immune = true;
					player.immuneTime = 20;
					player.velocity = Vector2.Normalize(startPos - hooked.Center) * 15;
					player.GetModPlayer<StarlightPlayer>().Shake += 15;

					hooked.StrikeNPC(projectile.damage, projectile.knockBack, player.Center.X < hooked.Center.X ? -1 : 1);

					Helper.PlayPitched("Guns/ChainPull", 0, 0, player.Center);
				}
			}

			if (struck)
			{
				player.fullRotation = (projectile.timeLeft / 20f) * 3.14f * player.direction;
				player.fullRotationOrigin = player.Size / 2;
				player.velocity *= 0.95f;
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			Player player = Main.player[projectile.owner];

			if (target.life <= 0)
				return;

			if (player.HeldItem.modItem is Scrapshot)
			{
				player.itemAnimation = 1;
				player.itemTime = 1;
			}

			{
				hooked = target;
				projectile.velocity *= 0;
				startPos = player.Center;
				Distance = Vector2.Distance(startPos, target.Center);
				projectile.friendly = false;
			}
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			damage /= 4;
			knockback /= 4f;
			crit = false;
			base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (struck)
				return false;

			Texture2D chainTex1 = ModContent.GetTexture(AssetDirectory.BreacherItem + "ScrapshotHookChain1");
			Texture2D chainTex2 = ModContent.GetTexture(AssetDirectory.BreacherItem + "ScrapshotHookChain2");
			Player player = Main.player[projectile.owner];

			float dist = Vector2.Distance(player.Center, projectile.Center);
			float rot = (player.Center - projectile.Center).ToRotation() + (float)Math.PI / 2f;


			float length = 1f / dist * chainTex1.Height;
			for (int k = 0; k * length < 1; k++)
			{
				var pos = Vector2.Lerp(projectile.Center, player.Center, k * length);
				if (k % 2 == 0)
					spriteBatch.Draw(chainTex1, pos - Main.screenPosition, null, lightColor, rot, chainTex1.Size() / 2, 1, 0, 0);
				else
					spriteBatch.Draw(chainTex2, pos - Main.screenPosition, null, lightColor, rot, chainTex1.Size() / 2, 1, 0, 0);
			}

			Texture2D hook = Main.projectileTexture[projectile.type];

			spriteBatch.Draw(hook, projectile.Center - Main.screenPosition, null, lightColor, rot + ((float)Math.PI * 0.75f), hook.Size() / 2, 1, 0, 0);

			return false;
		}
	}

	public class ScrapshotShrapnel : ModProjectile, IDrawPrimitive
	{
		public override string Texture => AssetDirectory.BreacherItem + "ExplosiveFlare";

		private List<Vector2> cache;

		private Trail trail;

		public override void SetDefaults()
		{
			projectile.width = 4;
			projectile.height = 4;
			projectile.ranged = true;
			projectile.friendly = true;
			projectile.penetrate = 1;
			projectile.timeLeft = 20;
			projectile.extraUpdates = 1;
			projectile.alpha = 255;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Explosive Shrapnel");
			Main.projFrames[projectile.type] = 2;
		}

		public override void AI()
		{
			if (projectile.timeLeft == 20)
				projectile.velocity *= Main.rand.NextFloat(12, 16);

			projectile.velocity *= 0.87f;
			ManageCaches();
			ManageTrail();
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 10; i++)
				{
					cache.Add(projectile.Center);
				}
			}

			cache.Add(projectile.Center);

			while (cache.Count > 10)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(40 * 4), factor => factor * 5, factor =>
			{
				return new Color(255, 170, 80) * factor.X * (projectile.timeLeft / 20f);
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = projectile.Center;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/GlowTrail"));

			trail?.Render(effect);
		}
	}
}