using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Projectiles;
using System;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Crimson
{
	internal class Lobotomizer : ModItem
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lobotomizer");
			Tooltip.SetDefault("Right click to throw a hallucinatory spear\nAttacks faster while hallucinating\nLonger reach while not hallucinating");
		}

		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.Melee;
			Item.width = 32;
			Item.height = 32;
			Item.damage = 34;
			Item.crit = 14;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 46;
			Item.useAnimation = 46;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.knockBack = 2f;
			Item.rare = ItemRarityID.Orange;
			Item.shoot = ModContent.ProjectileType<LobotomizerProjectile>();
			Item.shootSpeed = 1;

			Item.value = Item.sellPrice(gold: 1);
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override float UseTimeMultiplier(Player player)
		{
			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.ModProjectile is LobotomizerHallucination && Vector2.Distance(player.Center, proj.Center) <= (proj.ModProjectile as LobotomizerHallucination).Radius)
					return 0.18f;
			}

			return 1f;
		}

		public override float UseAnimationMultiplier(Player player)
		{
			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.ModProjectile is LobotomizerHallucination && Vector2.Distance(player.Center, proj.Center) <= (proj.ModProjectile as LobotomizerHallucination).Radius)
					return 0.18f;
			}

			return 1f;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.ModProjectile is LobotomizerHallucination && Vector2.Distance(player.Center, proj.Center) <= (proj.ModProjectile as LobotomizerHallucination).Radius)
				{
					type = ModContent.ProjectileType<LobotomizerFastProjectile>();
					velocity = velocity.RotatedByRandom(0.35f);
				}
			}
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse == 2)
			{
				foreach (Projectile proj in Main.ActiveProjectiles)
				{
					if (proj.type == ModContent.ProjectileType<LobotomizerHallucination>() && proj.owner == player.whoAmI)
						proj.timeLeft = 30;
				}

				Projectile.NewProjectile(source, position, velocity * 30f, ModContent.ProjectileType<LobotomizerHallucination>(), Item.damage, 0, player.whoAmI);
				return false;
			}

			return true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<DendriteBar>(), 10);
			recipe.AddIngredient(ModContent.ItemType<ImaginaryTissue>(), 5);
			recipe.AddIngredient(ItemID.TheRottedFork);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class LobotomizerProjectile : SpearProjectile
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public LobotomizerProjectile() : base(40, 60, 180)
		{
			isSymetrical = false;

			motionFunction = (n) =>
			{
				if (n < 0.15f)
					return n * n / 0.0225f * -0.1f;
				if (n < 0.3f)
					return -0.1f + 1.1f * ((n - 0.15f) * (n - 0.15f) / 0.0225f);
				if (n < 0.7f)
					return 1f + (n - 0.3f) / 0.4f * 0.1f;
				if (n <= 1f)
					return 1.1f - 1.1f * ((n - 0.7f) * (n - 0.7f) / 0.09f);

				return 0;
			};

			rotationOffset = 3.14f;
			origin = new Vector2(0, 15);
			fadeDuration = 7;
		}

		// Prevents insane multi hit while held out
		public override bool? CanHitNPC(NPC target)
		{
			float prog = 1f - Projectile.timeLeft / (float)RealDuration;
			return prog < 0.4f || prog > 0.7f ? null : false;
		}

		public override void SafeAI()
		{
			GraymatterBiome.forceGrayMatter = true;

			if (Projectile.timeLeft == RealDuration)
			{
				Helpers.Helper.PlayPitched("Effects/HeavyWhoosh", 1f, -0.5f + Main.rand.NextFloat(0.3f), Projectile.Center);
				Helpers.Helper.PlayPitched("Effects/HeavyWhooshShort", 1f, 1f + Main.rand.NextFloat(0.2f), Projectile.Center);
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = Assets.Misc.SpikeTell.Value;
			var source = new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height);

			float opacity = 1f;

			if (Projectile.timeLeft < 6)
				opacity = Projectile.timeLeft / 6f;

			if (Projectile.timeLeft > RealDuration - 6)
				opacity = (RealDuration - Projectile.timeLeft) / 6f;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, source, new Color(255, 255, 255, 0) * opacity * 0.1f, Projectile.rotation - 1.57f, new Vector2(tex.Width / 4f, tex.Height * 0.6f), 0.5f, 0, 0);

			float prog = 1f - Projectile.timeLeft / (float)RealDuration;
			float flashTime = prog < 0.1f ? prog / 0.1f : Math.Max(0, 1 - (prog - 0.1f) / 0.4f);

			tex = Assets.StarTexture.Value;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * opacity * (flashTime), Projectile.rotation - 1.57f + Projectile.timeLeft * 0.1f, tex.Size() / 2f, 0.2f + prog * 0.5f, 0, 0);

			Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.4f);
		}
	}

	public class LobotomizerFastProjectile : SpearProjectile
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public LobotomizerFastProjectile() : base(20, 60, 100)
		{
			isSymetrical = false;

			motionFunction = (n) =>
			{
				if (n < 0.15f)
					return n * n / 0.0225f * -0.1f;
				if (n < 0.3f)
					return -0.1f + 1.1f * ((n - 0.15f) * (n - 0.15f) / 0.0225f);
				if (n < 0.7f)
					return 1f + (n - 0.3f) / 0.4f * 0.1f;
				if (n <= 1f)
					return 1.1f - 1.1f * ((n - 0.7f) * (n - 0.7f) / 0.09f);

				return 0;
			};

			rotationOffset = 3.14f;
			origin = new Vector2(0, 15);

			fadeDuration = 10;
		}

		public override void SafeAI()
		{
			Projectile.usesLocalNPCImmunity = true;
			Projectile.aiStyle = -1;

			if (Projectile.timeLeft == RealDuration)
			{
				Helpers.Helper.PlayPitched("Effects/HeavyWhooshShort", 1f, 0.4f + Main.rand.NextFloat(0.6f), Projectile.Center);
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = Assets.Misc.SpikeTell.Value;
			var source = new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height);

			float opacity = 1f;
			int realDuration = (Projectile.ModProjectile as SpearProjectile).RealDuration;

			if (Projectile.timeLeft < 6)
				opacity = Projectile.timeLeft / 6f;

			if (Projectile.timeLeft > realDuration - 6)
				opacity = (realDuration - Projectile.timeLeft) / 6f;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, source, new Color(255, 255, 255, 0) * opacity * 0.05f, Projectile.rotation - 1.57f, new Vector2(tex.Width / 4f, tex.Height * 0.5f), 0.8f, 0, 0);

			float prog = 1f - Projectile.timeLeft / (float)realDuration;
			float flashTime = prog < 0.2f ? prog / 0.2f : Math.Max(0, 1 - (prog - 0.2f) / 0.5f);

			tex = Assets.StarTexture.Value;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * opacity * (flashTime), Projectile.rotation - 1.57f + Projectile.ai[2] + Projectile.timeLeft * 0.2f, tex.Size() / 2f, 0.1f + prog * 0.3f, 0, 0);
		}
	}

	public class LobotomizerHallucination : ModProjectile
	{
		public Vector2 savedOff;
		public NPC embedded;

		public override string Texture => AssetDirectory.CrimsonItem + "LobotomizerProjectile";

		public ref float Radius => ref Projectile.ai[0];
		public ref float State => ref Projectile.ai[1];

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawSpearHallucinations;
		}

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 10;
			ProjectileID.Sets.TrailingMode[Type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.timeLeft = 400;
			Projectile.width = 15;
			Projectile.height = 15;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.position += Projectile.velocity * 2;
			Projectile.velocity *= 0;
			Projectile.friendly = false;

			State = 1;

			Impact();

			return false;
		}

		public override void AI()
		{
			GraymatterBiome.forceGrayMatter = true;

			if (State == 0)
			{
				Projectile.velocity.Y += 0.4f;
				Projectile.velocity.X *= 0.99f;
				Projectile.rotation = Projectile.velocity.ToRotation() - 3.14f;
			}
			else if (State == 1)
			{
				Projectile.velocity *= 0;
			}
			else if (State == 2)
			{
				if (!embedded.active || embedded is null)
				{
					State = 0;
					Projectile.timeLeft = 30;
				}

				Projectile.Center = embedded.Center + savedOff;
			}

			var maxRad = State == 1 ? 300 : 200;

			if (State > 0 && Radius < maxRad && Projectile.timeLeft > 30)
				Radius += 10;

			if (Radius > 0 && Projectile.timeLeft <= 30)
				Radius -= 10;

			// Allows this to make it's owner hallucinate
			Player player = Main.player[Projectile.owner];

			Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 1.5f);
		}

		public void Impact()
		{
			for(int k = 0; k < 10; k++)
			{
				Dust.NewDustPerfect(Projectile.Center, DustID.Dirt);
			}

			Helpers.Helper.PlayPitched("Impacts/StabTiny", 1f, -0.5f, Projectile.Center);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.position += Projectile.velocity * 2;
			Projectile.velocity *= 0;
			Projectile.friendly = false;

			savedOff = Projectile.Center - target.Center;
			embedded = target;
			State = 2;

			Impact();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = Assets.Items.Crimson.LobotomizerFastProjectile.Value;
			float opacity = Projectile.timeLeft > 30f ? 1f : (Radius / 300f);
			var pos = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * tex.Width / 2f - Main.screenPosition;

			Main.spriteBatch.Draw(tex, pos, tex.Frame(), new Color(255, 255, 255, 0) * opacity, Projectile.rotation, tex.Size() / 2f, Projectile.scale, 0, 0);
			return false;
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = Assets.Misc.GlowRing.Value;
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * 0.1f, 0, tex.Size() / 2f, Radius * 2f / tex.Width, 0, 0);
		}

		private void DrawSpearHallucinations(SpriteBatch batch)
		{
			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.type == Type)
				{
					Texture2D tex = Assets.Misc.StarView.Value;
					batch.Draw(tex, proj.Center - Main.screenPosition, null, new Color(255, 255, 255, 0), 0, tex.Size() / 2f, (proj.ModProjectile as LobotomizerHallucination).Radius * 7f / tex.Width, 0, 0);
				}

				if (proj.type == ModContent.ProjectileType<LobotomizerProjectile>() || proj.type == ModContent.ProjectileType<LobotomizerFastProjectile>())
				{
					Texture2D tex = Assets.Misc.SpikeTell.Value;
					var source = new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height);

					float opacity = 1f;
					int realDuration = (proj.ModProjectile as SpearProjectile).RealDuration;

					if (proj.timeLeft < 6)
						opacity = Projectile.timeLeft / 6f;

					if (Projectile.timeLeft > realDuration - 6)
						opacity = (realDuration - proj.timeLeft) / 6f;

					batch.Draw(tex, proj.Center - Main.screenPosition, source, new Color(255, 255, 255, 0) * opacity, proj.rotation - 1.57f, new Vector2(tex.Width / 4f, tex.Height * 0.6f), 1f, 0, 0);
				}
			}
		}
	}
}
