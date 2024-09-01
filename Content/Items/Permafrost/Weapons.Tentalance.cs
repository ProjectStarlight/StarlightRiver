using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Permafrost
{
	internal class Tentalance : ModItem
	{
		public override string Texture => AssetDirectory.PermafrostItem + Name;

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Launches a shimmering tentacle lance\nCharge to fire up to 4 more lances at once");
		}

		public override void SetDefaults()
		{
			Item.width = 54;
			Item.height = 54;
			Item.DamageType = DamageClass.Melee;
			Item.damage = 18;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = Terraria.ID.ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.channel = true;
			Item.shoot = ModContent.ProjectileType<TentalanceProjectile>();
			Item.shootSpeed = 60;
			Item.rare = Terraria.ID.ItemRarityID.Green;
			Item.autoReuse = true;

			Item.value = Item.sellPrice(gold: 2);
		}

		public Color GetColor(float off)
		{
			float sin = 1 + (float)Math.Sin(off);
			float cos = 1 + (float)Math.Cos(off);
			return new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
		}

		public override bool CanShoot(Player player)
		{
			return player.ownedProjectileCounts[ModContent.ProjectileType<TentalanceProjectile>()] == 0;
		}
	}

	internal class TentalanceProjectile : ModProjectile
	{
		private List<Vector2> cache;
		private Trail trail;

		public Player Owner => Main.player[Projectile.owner];
		public ref float Timer => ref Projectile.ai[0];
		public ref float ChargeSnapshot => ref Projectile.ai[1];
		public ref float Charge => ref Projectile.ai[2];

		public override string Texture => AssetDirectory.PermafrostItem + Name;

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = 120;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			Projectile.DamageType = DamageClass.Melee;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (ChargeSnapshot <= 0)
				return false;

			return base.CanHitNPC(target);
		}

		public override void AI()
		{
			Owner.itemTime = Projectile.timeLeft; //lock player from switching off until this is done
			Owner.itemAnimation = Projectile.timeLeft;

			if (!Owner.channel || Timer > 0)
			{
				Timer++;

				if (ChargeSnapshot <= 0)
				{
					ChargeSnapshot = Charge;
					Projectile.damage += (int)Charge;
					Helper.PlayPitched("SquidBoss/LightSplash", 0.2f, -0.5f, Owner.Center);
					if (ChargeSnapshot <= 0)
						ChargeSnapshot = 1; //make sure it atleast does damage on its attack
				}
			}
			else
			{
				if (Owner.channel && Charge == 29)
				{
					Helper.PlayPitched("MagicAttack", 1, 1, Owner.Center);

					for (int k = 0; k < 40; k++)
					{
						Dust.NewDustPerfect(Owner.Center, ModContent.DustType<Dusts.Cinder>(), Vector2.One.RotatedBy(k / 40f * 6.28f) * 1.5f, 0, GetColor(k / 40f * 6.28f * 3), Main.rand.NextFloat(0.5f, 1));
					}

					Charge = 30;
				}

				if (Owner.channel && Charge < 30)
					Charge++;

				Projectile.timeLeft = 120;
				Owner.TryGetModPlayer(out ControlsPlayer controlsPlayer);
				controlsPlayer.mouseRotationListener = true;
				Projectile.velocity = Vector2.Normalize(controlsPlayer.mouseWorld - Owner.Center) * Projectile.velocity.Length();

				Owner.SetCompositeArmFront(true, 0, Projectile.rotation - 1.57f);
				Owner.direction = Projectile.velocity.X > 0 ? 1 : -1;
			}

			if (Timer == 10 && ChargeSnapshot >= 15)
			{
				if (Main.myPlayer == Projectile.owner)
				{
					Projectile.NewProjectile(Owner.GetSource_ItemUse(Owner.HeldItem), Projectile.Center, Projectile.velocity.RotatedBy(0.25f), Type, Projectile.damage / 2, Projectile.knockBack, Projectile.owner, 0, 14);
					Projectile.NewProjectile(Owner.GetSource_ItemUse(Owner.HeldItem), Projectile.Center, Projectile.velocity.RotatedBy(-0.25f), Type, Projectile.damage / 2, Projectile.knockBack, Projectile.owner, 0, 14);
				}

				Helper.PlayPitched("SquidBoss/LightSplash", 0.3f, -0.5f, Owner.Center);
			}

			if (Timer == 20 && ChargeSnapshot >= 30)
			{
				if (Main.myPlayer == Projectile.owner)
				{
					Projectile.NewProjectile(Owner.GetSource_ItemUse(Owner.HeldItem), Projectile.Center, Projectile.velocity.RotatedBy(0.45f), Type, Projectile.damage / 2, Projectile.knockBack, Projectile.owner, 0, 1);
					Projectile.NewProjectile(Owner.GetSource_ItemUse(Owner.HeldItem), Projectile.Center, Projectile.velocity.RotatedBy(-0.45f), Type, Projectile.damage / 2, Projectile.knockBack, Projectile.owner, 0, 1);
				}

				Helper.PlayPitched("SquidBoss/SuperSplash", 0.5f, -0.5f, Owner.Center);
			}

			Vector2 basePos = Owner.Center + Vector2.UnitY * Owner.gfxOffY;
			float dist = (float)Math.Sin(Timer / 120f * 3.14f) - Charge / 30f * 0.1f;
			float rot = (float)Math.Sin(Timer / 120f * 6.28f) * 0.05f;
			Projectile.Center = basePos + dist * Projectile.velocity.RotatedBy(rot) * (4 + ChargeSnapshot / 7.5f);

			Projectile.rotation = Projectile.velocity.ToRotation();

			Lighting.AddLight(Projectile.Center, GetColor(Charge * 0.1f).ToVector3());

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.damage /= 2;
		}

		public Color GetColor(float off)
		{
			float sin = 1 + (float)Math.Sin(Timer * 0.1f + off);
			float cos = 1 + (float)Math.Cos(Timer * 0.1f + off);
			return new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Vector2 basePos = Owner.Center + Vector2.UnitY * Owner.gfxOffY;
			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D tex = Assets.Items.Permafrost.TentalanceProjectile.Value;
			Texture2D texGlow = Assets.Items.Permafrost.TentalanceProjectileGlow.Value;
			Texture2D texGlow2 = Assets.Items.Permafrost.TentalanceProjectileGlow2.Value;

			spriteBatch.Draw(tex, Projectile.Center - Projectile.velocity - Main.screenPosition, null, lightColor, Projectile.rotation + 1.57f / 2, tex.Size() / 2, 1, 0, 0);

			spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation + 1.57f / 2, texGlow.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(texGlow2, Projectile.Center - Main.screenPosition, null, GetColor(Charge * 0.1f), Projectile.rotation + 1.57f / 2, texGlow2.Size() / 2, 1, 0, 0);

			for (int k = 0; k < 24; k++)
			{
				var pos = Vector2.Lerp(basePos, Projectile.Center, k / 29f);
				pos += Vector2.Normalize(Projectile.velocity).RotatedBy(1.57f) * (float)Math.Sin(k / 24f * 6.28f * 2) * 5;
				Texture2D texSegment = Assets.Items.Permafrost.TentalanceProjectileSegment.Value;

				spriteBatch.Draw(texSegment, pos - Main.screenPosition, null, Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16), Projectile.rotation, texSegment.Size() / 2, 1, 0, 0);
			}

			//Render trail
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.01f);
			effect.Parameters["repeats"].SetValue(1);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.EnergyTrail.Value);

			trail?.Render(effect);

			return false;
		}

		private void ManageCaches()
		{
			if (cache is null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 60; i++)
				{
					cache.Add(Vector2.Zero);
				}
			}

			Vector2 basePos = Owner.Center + Vector2.UnitY * Owner.gfxOffY;
			double sinTime = Timer / 120f * Math.PI * (4 + ChargeSnapshot / 15f) * 2;
			float amplitude = 0.25f * Math.Max(0, (Projectile.timeLeft - 60) / 60f);
			cache.Add(Projectile.Center - basePos + Projectile.velocity.RotatedBy(1.57f) * (float)Math.Sin(sinTime) * amplitude);

			while (cache.Count > 60)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			if (trail is null || trail.IsDisposed)
				trail = new Trail(Main.instance.GraphicsDevice, 60, new NoTip(), factor => 15, factor => GetColor(factor.X) * factor.X * (float)Math.Sin(Math.Max(0, Projectile.timeLeft - 30) / 90f * 3.14f));

			var realCache = new Vector2[60];

			for (int k = 0; k < 60; k++)
			{
				realCache[k] = cache[k] + Owner.Center + Vector2.UnitY * Owner.gfxOffY;
			}

			trail.Positions = realCache;

			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}
	}
}