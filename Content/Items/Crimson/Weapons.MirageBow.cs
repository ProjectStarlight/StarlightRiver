using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Items.Vitric;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
namespace StarlightRiver.Content.Items.Crimson
{
	internal class MirageBow : ModItem
	{
		private int cloneCooldown = 0;

		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public override void Load()
		{
			StarlightProjectile.OnHitNPCEvent += SpawnOnCrit;
		}

		public override void SetDefaults()
		{
			Item.damage = 24;
			Item.crit = 10;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 32;
			Item.height = 52;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 1;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 5, 0, 0);
			Item.shoot = ProjectileID.PurificationPowder;
			Item.shootSpeed = 10f;
			Item.autoReuse = true;
			Item.useAmmo = AmmoID.Arrow;
			Item.useTurn = true;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item5;
		}

		public override void UpdateInventory(Player player)
		{
			if (cloneCooldown > 0)
				cloneCooldown--;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// spawn held
			if (!Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<MirageBowHeld>() && n.owner == player.whoAmI))
				Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<MirageBowHeld>(), 0, 0, player.whoAmI);

			Projectile clone = Main.projectile.FirstOrDefault(n => n.active && n.type == ModContent.ProjectileType<MirageBowClone>() && n.owner == player.whoAmI);

			if (clone is null)
				return true;

			Projectile.NewProjectile(Item.GetSource_FromThis(), clone.Center, clone.Center.DirectionTo(Main.MouseWorld) * Item.shootSpeed * 2, type, damage / 2, knockback, player.whoAmI);
			return true;
		}

		private void SpawnOnCrit(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player owner = Main.player[projectile.owner];

			if (owner.active && owner.HeldItem.type == ModContent.ItemType<MirageBow>() && hit.Crit)
			{
				var moditem = owner.HeldItem.ModItem as MirageBow;

				if (moditem is null)
					return;

				if (moditem.cloneCooldown <= 0)
				{
					Projectile.NewProjectile(owner.HeldItem.GetSource_FromThis(), owner.Center, Vector2.Zero, ModContent.ProjectileType<MirageBowClone>(), 1, 0, owner.whoAmI);
					moditem.cloneCooldown = 60 * 8;
				}
			}
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Texture2D tex = Assets.Items.Crimson.MirageBow.Value;

			Effect effect = Filters.Scene["MirageItemFilter"].GetShader().Shader;

			effect.Parameters["u_color"].SetValue(Vector3.One);
			effect.Parameters["u_fade"].SetValue(Vector3.One);
			effect.Parameters["u_resolution"].SetValue(tex.Size());
			effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.1f);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, SamplerState.LinearClamp, default, default, effect, Main.UIScaleMatrix);

			spriteBatch.Draw(tex, position, frame, drawColor, 0, origin, scale, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.LinearClamp, default, default, default, Main.UIScaleMatrix);

			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D tex = Assets.Items.Crimson.MirageBow.Value;

			Effect effect = Filters.Scene["MirageItemFilter"].GetShader().Shader;

			effect.Parameters["u_color"].SetValue(Vector3.One);
			effect.Parameters["u_fade"].SetValue(Vector3.One);
			effect.Parameters["u_resolution"].SetValue(tex.Size());
			effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.05f);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, SamplerState.LinearClamp, default, default, effect, Main.UIScaleMatrix);

			spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, Color.White, rotation, Item.Size / 2f, scale, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.LinearClamp, default, default, default, Main.UIScaleMatrix);

			return false;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<DendriteBar>(), 10);
			recipe.AddIngredient(ModContent.ItemType<ImaginaryTissue>(), 5);
			recipe.AddIngredient(ItemID.TendonBow);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	internal class MirageBowHeld : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 10;
		}

		public override void AI()
		{
			Player owner = Projectile.Owner();

			if (owner.itemAnimation == owner.itemAnimationMax)
				Projectile.rotation = Projectile.Center.DirectionTo(Main.MouseWorld).ToRotation();

			if (owner.itemAnimation > 0)
				Projectile.timeLeft = 10;

			owner.direction = Projectile.rotation.ToRotationVector2().X > 0 ? 1 : -1;

			Projectile.Center = owner.Center + Vector2.UnitY * owner.gfxOffY;
			owner.heldProj = Projectile.whoAmI;

			Color glowColor = new Color(
				1.3f + MathF.Sin(Main.GameUpdateCount / 60f * 3.14f) * 0.5f,
				1.3f + MathF.Sin(Main.GameUpdateCount / 60f * 3.14f + 1) * 0.5f,
				1.3f + MathF.Sin(Main.GameUpdateCount / 60f * 3.14f + 2) * 0.5f);

			Lighting.AddLight(owner.Center, glowColor.ToVector3() * 0.5f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = Assets.Items.Crimson.MirageBow.Value;
			var origin = new Vector2(0, tex.Height / 2);
			SpriteEffects direction = Projectile.rotation.ToRotationVector2().X > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

			Effect effect = Filters.Scene["MirageItemFilter"].GetShader().Shader;

			effect.Parameters["u_color"].SetValue(Vector3.One);
			effect.Parameters["u_fade"].SetValue(Vector3.One);
			effect.Parameters["u_resolution"].SetValue(tex.Size());
			effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.1f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.NonPremultiplied, Main.DefaultSamplerState, default, default, effect, Main.UIScaleMatrix);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, 1, direction, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, default, default, Main.UIScaleMatrix);

			return false;
		}
	}

	internal class MirageBowClone : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawCloneHallucinations;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 60 * 5;
		}

		public override void AI()
		{
			GraymatterBiome.forceGrayMatter = true;

			Player owner = Main.player[Projectile.owner];

			float glow = Projectile.timeLeft > 270 ? 1f - (Projectile.timeLeft - 270) / 30f : Projectile.timeLeft / 270f;
			Lighting.AddLight(Projectile.Center, Vector3.One * 0.5f * glow);
			Lighting.AddLight(owner.Center, Vector3.One * 0.5f * glow);

			if (owner.HeldItem.type != ModContent.ItemType<MirageBow>())
				Projectile.timeLeft = 30;

			if (owner.itemAnimation == owner.itemAnimationMax)
				Projectile.rotation = Projectile.Center.DirectionTo(Main.MouseWorld).ToRotation();

			if (owner == Main.LocalPlayer)
			{
				Vector2 relative = owner.Center - Main.MouseWorld;
				Vector2 target = Main.MouseWorld + relative.RotatedBy(3.14f);
				Projectile.Center += (target - Projectile.Center) * 0.1f;
				Projectile.netUpdate = true;
			}
		}

		public override bool? CanHitNPC(NPC target)
		{
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (!PlayerTarget.canUseTarget)
				return false;

			Player owner = Main.player[Projectile.owner];
			Rectangle source = PlayerTarget.getPlayerTargetSourceRectangle(Projectile.owner);

			float alpha = Projectile.timeLeft < 30 ? Projectile.timeLeft / 30f * 0.5f : 0.5f;

			Main.spriteBatch.Draw(PlayerTarget.Target, Projectile.Center - Main.screenPosition, source, Color.White * alpha, owner.fullRotation, source.Size() / 2f + new Vector2(-owner.width, owner.height) / 2f, 1, SpriteEffects.FlipHorizontally, 0);

			Texture2D tex = Assets.Items.Crimson.MirageBow.Value;
			var origin = new Vector2(0, tex.Height / 2);
			SpriteEffects direction = Projectile.rotation.ToRotationVector2().X > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

			Effect effect = Filters.Scene["MirageItemFilter"].GetShader().Shader;

			effect.Parameters["u_color"].SetValue(Vector3.One);
			effect.Parameters["u_fade"].SetValue(Vector3.One);
			effect.Parameters["u_resolution"].SetValue(tex.Size());
			effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.1f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.NonPremultiplied, Main.DefaultSamplerState, default, default, effect, Main.UIScaleMatrix);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * alpha, Projectile.rotation, origin, 1, direction, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, default, default, Main.UIScaleMatrix);

			return false;
		}

		private void DrawCloneHallucinations(SpriteBatch batch)
		{
			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.type == Type)
				{
					Texture2D tex = Assets.Keys.GlowAlpha.Value;
					float alpha = proj.timeLeft < 30 ? proj.timeLeft / 30f * 0.5f : 0.5f;

					batch.Draw(tex, proj.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * alpha, 0, tex.Size() / 2f, 2.5f, 0, 0);
				}
			}
		}
	}
}