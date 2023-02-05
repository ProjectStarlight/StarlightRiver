using StarlightRiver.Content.Dusts;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	public class MagmaArrow : ModItem
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magma Arrow");
			Tooltip.SetDefault("Becomes more cooled as it travels");
		}

		public override void SetDefaults()
		{
			Item.damage = 8;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 8;
			Item.height = 8;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.knockBack = 0.5f;
			Item.value = 10;
			Item.rare = ItemRarityID.Green;
			Item.shoot = ProjectileType<MagmaArrowProj>();
			Item.shootSpeed = 6f;
			Item.ammo = AmmoID.Arrow;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe(100);
			recipe.AddIngredient(ItemID.FlamingArrow, 100);
			recipe.AddIngredient(ItemType<MagmaCore>(), 1);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	internal class MagmaArrowProj : ModProjectile
	{
		private float progressToDepletion = 0;
		private float moltenCounter = 1;

		private float magmaRemaining => 1 - progressToDepletion;

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magma Arrow");
		}

		public override void SetDefaults()
		{
			Projectile.width = 7;
			Projectile.height = 7;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 400;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
			Projectile.aiStyle = 1;
			AIType = ProjectileID.WoodenArrowFriendly;
		}

		public override void AI()
		{
			if (progressToDepletion < 1 && moltenCounter <= 0)
				progressToDepletion += 0.01f;

			if (moltenCounter > 0)
				moltenCounter -= 0.025f;

			Lighting.AddLight(Projectile.Center, (Color.Orange * magmaRemaining).ToVector3());

			if (Main.rand.NextFloat() > System.MathF.Sqrt(progressToDepletion) && Main.rand.NextBool(9) && magmaRemaining > 0.25f)
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(1, 1), ModContent.ProjectileType<ArrowMagma>(), Projectile.damage / 2, 0, Projectile.owner);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D tex = Request<Texture2D>(AssetDirectory.VitricItem + "NeedlerBloom").Value;
			Color bloomColor = Color.Orange;
			bloomColor.A = 0;

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY) + (Projectile.rotation - 1.57f).ToRotationVector2() * 10, new Rectangle(0, 0, tex.Width, tex.Height), //bloom
				bloomColor * magmaRemaining, Projectile.rotation, new Vector2(tex.Width / 2, tex.Height / 2), Projectile.scale * 0.85f, SpriteEffects.None, 0);

			tex = TextureAssets.Projectile[Projectile.type].Value;

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Rectangle(tex.Width / 3, 0, tex.Width / 3, tex.Height), //crystal arrow
				lightColor * progressToDepletion, Projectile.rotation, new Vector2(tex.Width / 6, tex.Height / 2), Projectile.scale, SpriteEffects.None, 0);

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Rectangle(0, 0, tex.Width / 3, tex.Height), //magma arrow
				lightColor * magmaRemaining, Projectile.rotation, new Vector2(tex.Width / 6, tex.Height / 2), Projectile.scale, SpriteEffects.None, 0);

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Rectangle(tex.Width / 3 * 2, 0, tex.Width / 3, tex.Height), //white sprite
				Color.Lerp(Color.Orange, Color.White, moltenCounter) * moltenCounter, Projectile.rotation, new Vector2(tex.Width / 6, tex.Height / 2), Projectile.scale, SpriteEffects.None, 0);

			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (magmaRemaining > 0.4f)
				target.AddBuff(BuffID.OnFire, 180);
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			damage += (int)(progressToDepletion * 8);
			knockback *= 1 + progressToDepletion;
		}

		public override void Kill(int timeLeft)
		{
			var unused1 = SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);

			for (int k = 1; k <= 6; k++)
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), ModContent.DustType<Glow>(), Main.rand.NextVector2Circular(4, 4), 0, Color.Orange, 0.5f);
		}
	}

	internal class ArrowMagma : ModProjectile
	{ 

		private List<Vector2> oldPos = new();

		public override string Texture => AssetDirectory.Keys + "GlowHarshAlpha";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magma Arrow");
		}

		public override void SetDefaults()
		{
			Projectile.width = 4;
			Projectile.height = 4;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 150;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Color color = Color.Orange;
			color.A = 0;
			color *= 0.2f;
			for (int i = 1; i < oldPos.Count; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					Main.spriteBatch.Draw(tex, Vector2.Lerp(oldPos[i], oldPos[i - 1], j / 5f) - Main.screenPosition, null, color, 0, tex.Size() / 2, (i + 4) / 8f * Projectile.scale, SpriteEffects.None, 0f);

				}
			}

			color = new Color(255, 255, 255, 0);
			for (int i = 1; i < oldPos.Count; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					Main.spriteBatch.Draw(tex, Vector2.Lerp(oldPos[i], oldPos[i - 1], j / 5f) - Main.screenPosition, null, color, 0, tex.Size() / 2, (i + 4) / 8f * Projectile.scale * 0.2f, SpriteEffects.None, 0f);

				}
			}

			return false;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.scale = Main.rand.NextFloat(0.3f, 0.6f);
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.1f;

			oldPos.Add(Projectile.Center);
			while (oldPos.Count > 4)
			{
				oldPos.RemoveAt(0);
			}

			if (Projectile.timeLeft < 15)
				Projectile.scale *= 0.9f;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.timeLeft > 15)
				Projectile.timeLeft = 15;

			Projectile.velocity = Vector2.Zero;
			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 180);
		}
	}
}