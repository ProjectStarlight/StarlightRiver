using Terraria.ID;

namespace StarlightRiver.Content.Items
{
	class DebugStick : ModItem
	{
		public NPC target;
		public Projectile target2;
		public Player owner;

		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Debug Stick");
			Tooltip.SetDefault("Has whatever effects are needed");
		}

		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.DamageType = DamageClass.Melee;
			Item.width = 38;
			Item.height = 40;
			Item.useTime = 18;

			Item.useAnimation = 18;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5f;
			Item.value = 1000;
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item18;
			Item.useTurn = true;
			Item.accessory = true;
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Vector2 pos = new(Main.screenWidth / 2, Main.screenHeight / 2);
			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Permafrost/WaterStaff").Value;

			DrawHelper.DrawWithPerspective(spriteBatch, tex, pos, Color.White, 1, d, a, b, c, 0.1f);
		}

		public override void UpdateInventory(Player player)
		{
			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.I))
				a += 0.3f;

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.U))
				a -= 0.3f;

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.K))
				b += 0.3f;

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.J))
				b -= 0.3f;

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.M))
				c += 0.1f;

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.N))
				c -= 0.1f;

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.P))
				d += 0.1f;

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.O))
				d -= 0.1f;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{

		}

		public override bool? UseItem(Player player)
		{
			StarlightWorld.ShrineGen(null, null);
			return true;
		}
	}

	class DebugModerEnabler : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Debug Mode");
			Tooltip.SetDefault("Enables debug mode");
		}

		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.width = 38;
			Item.height = 38;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.useStyle = ItemUseStyleID.Swing;
		}

		public override bool? UseItem(Player Player)
		{
			StarlightRiver.debugMode = !StarlightRiver.debugMode;
			return true;
		}
	}
}