using StarlightRiver.Helpers;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Utility
{
	class LaserPointerLoader : IOrderedLoadable
	{
		public float Priority => 1;

		public void Load()
		{
			StarlightRiver Mod = StarlightRiver.Instance;

			Mod.AddContent(new LaserPointer("RedLaserPointer", new Color(255, 20, 20), ItemID.Ruby));
			Mod.AddContent(new LaserPointer("OrangeLaserPointer", new Color(255, 120, 20), ItemID.Amber));
			Mod.AddContent(new LaserPointer("YellowLaserPointer", new Color(255, 255, 20), ItemID.Topaz));
			Mod.AddContent(new LaserPointer("GreenLaserPointer", new Color(20, 255, 20), ItemID.Emerald));
			Mod.AddContent(new LaserPointer("BlueLaserPointer", new Color(20, 80, 255), ItemID.Sapphire));
			Mod.AddContent(new LaserPointer("PurpleLaserPointer", new Color(150, 20, 255), ItemID.Amethyst));
			Mod.AddContent(new LaserPointer("WhiteLaserPointer", Color.White, ItemID.Diamond));
			Mod.AddContent(new LaserPointer("PinkLaserPointer", new Color(255, 140, 180), ItemID.PinkGel));
		}

		public void Unload() { }
	}

	[Autoload(false)]
	class LaserPointer : ModItem
	{
		private Color color;
		private readonly int extraMaterial;
		private readonly string InternalName;

		protected override bool CloneNewInstances => true;

		public override string Name => InternalName;

		public override string Texture => "StarlightRiver/Assets/Items/Utility/LaserPointer";

		public LaserPointer() { }

		public LaserPointer(string internalName, Color color, int extraMaterial)
		{
			InternalName = internalName;
			this.color = color;
			this.extraMaterial = extraMaterial;
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.rare = ItemRarityID.Green;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 5;
			Item.useAnimation = 5;
			Item.channel = true;
			Item.shoot = ModContent.ProjectileType<LaserPointerProjectile>();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (!Main.projectile.Any(n => n.active && n.type == Item.shoot && n.owner == player.whoAmI))
			{
				var p = Projectile.NewProjectileDirect(source, player.Center, Vector2.Zero, ModContent.ProjectileType<LaserPointerProjectile>(), damage, knockback, player.whoAmI);
				(p.ModProjectile as LaserPointerProjectile).color = color;
			}

			return false;
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture + "Glow").Value;
			spriteBatch.Draw(tex, position, frame, color, 0, origin, scale, 0, 0);
		}

		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture + "Glow").Value;
			spriteBatch.Draw(tex, Item.position, null, color, 0, Vector2.Zero, scale, 0, 0);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 5);
			recipe.AddIngredient(extraMaterial);
			recipe.Register();
		}
	}

	class LaserPointerProjectile : ModProjectile, IDrawAdditive
	{
		public Vector2 endPoint;
		public float LaserRotation;
		public Color color;

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => "StarlightRiver/Assets/Items/Utility/LaserPointerProjectile";

		public override void SetDefaults()
		{
			Projectile.timeLeft = 2;
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			Projectile.Center = Owner.Center;
			LaserRotation = (Main.MouseWorld - Owner.Center).ToRotation();
			Owner.heldProj = Projectile.whoAmI;

			if (Owner.channel)
				Projectile.timeLeft = 2;

			for (int k = 0; k < 160; k++)
			{
				Vector2 posCheck = Projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * k * 8;

				if (Helper.PointInTile(posCheck) || k == 159)
				{
					endPoint = posCheck;
					break;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, Owner.Center - Main.screenPosition, null, lightColor, LaserRotation, new Vector2(0, ModContent.Request<Texture2D>(Texture).Value.Height - 3), 1, 0, 0);
			Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture + "Glow").Value, Owner.Center - Main.screenPosition, null, color, LaserRotation, new Vector2(0, ModContent.Request<Texture2D>(Texture + "Glow").Value.Height - 3), 1, 0, 0);
			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D texBeam = ModContent.Request<Texture2D>(AssetDirectory.Assets + "GlowTrail").Value;

			var origin = new Vector2(0, texBeam.Height / 2);

			float height = 10;
			int width = (int)(Projectile.Center - endPoint).Length() - 24;

			Vector2 pos = Projectile.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(LaserRotation) * 24;
			var target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 1.2f));
			spriteBatch.Draw(texBeam, target, null, color, LaserRotation, origin, 0, 0);

			for (int i = 0; i < width; i += 10)
				Lighting.AddLight(pos + Vector2.UnitX.RotatedBy(LaserRotation) * i + Main.screenPosition, color.ToVector3() * height * 0.030f);

			Texture2D impactTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Keys/GlowSoft").Value;
			spriteBatch.Draw(impactTex, endPoint - Main.screenPosition, null, color, 0, impactTex.Size() / 2, 0.5f, 0, 0);
		}
	}
}
