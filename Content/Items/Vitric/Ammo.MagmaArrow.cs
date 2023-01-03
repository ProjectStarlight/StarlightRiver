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
		private float coolness = 0;
		private float moltenCounter = 1;

		private float Heat => 1 - coolness;

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
			if (coolness < 1 && moltenCounter <= 0)
				coolness += 0.015f;

			if (moltenCounter > 0)
				moltenCounter -= 0.05f;

			Lighting.AddLight(Projectile.Center, (Color.Orange * Heat).ToVector3());

			if (coolness < 1 && Main.rand.NextBool(10))
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ArrowMagma>(), Projectile.damage / 3, 0, Projectile.owner);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D tex = Request<Texture2D>(AssetDirectory.VitricItem + "NeedlerBloom").Value;
			Color bloomColor = Color.Orange;
			bloomColor.A = 0;

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY) + (Projectile.rotation - 1.57f).ToRotationVector2() * 10, new Rectangle(0, 0, tex.Width, tex.Height), //bloom
				bloomColor * Heat, Projectile.rotation, new Vector2(tex.Width / 2, tex.Height / 2), Projectile.scale * 0.85f, SpriteEffects.None, 0);

			tex = TextureAssets.Projectile[Projectile.type].Value;

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Rectangle(tex.Width / 3, 0, tex.Width / 3, tex.Height), //crystal arrow
				lightColor * coolness, Projectile.rotation, new Vector2(tex.Width / 6, tex.Height / 2), Projectile.scale, SpriteEffects.None, 0);

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Rectangle(0, 0, tex.Width / 3, tex.Height), //magma arrow
				lightColor * Heat, Projectile.rotation, new Vector2(tex.Width / 6, tex.Height / 2), Projectile.scale, SpriteEffects.None, 0);

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Rectangle(tex.Width / 3 * 2, 0, tex.Width / 3, tex.Height), //white sprite
				Color.Lerp(Color.Orange, Color.White, moltenCounter) * moltenCounter, Projectile.rotation, new Vector2(tex.Width / 6, tex.Height / 2), Projectile.scale, SpriteEffects.None, 0);

			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (Heat > 0.4f)
				target.AddBuff(BuffID.OnFire, 180);
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			damage += (int)(coolness * 8);
			knockback *= 1 + coolness;
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
		}
	}

	internal class ArrowMagma : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magma Arrow");
		}

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 150;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.scale = Main.rand.NextFloat(0.3f, 0.5f);
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.1f;

			if (Projectile.timeLeft < 20)
				Projectile.scale *= 0.9f;
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.velocity = Vector2.Zero;
			return false;
		}
	}
}