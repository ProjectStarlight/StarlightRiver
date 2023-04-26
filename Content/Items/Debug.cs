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

		public override void UpdateAccessory(Player player, bool hideVisual)
		{

		}

		public override bool? UseItem(Player player)
		{
<<<<<<< HEAD
=======
			player.GetModPlayer<Abilities.AbilityHandler>().Lock<Abilities.ForbiddenWinds.Dash>();

			var start = (Main.MouseWorld / 16).ToPoint16();

			for (int x = 0; x < 20; x++)
			{
				for (int y = 0; y < 20; y++)
				{
					Tile tile = Framing.GetTileSafely(start.X + x, start.Y + y);

					if (tile.WallType == ModContent.WallType<Tiles.Vitric.AncientSandstoneWall>())
						tile.WallType = (ushort)ModContent.WallType<Tiles.Vitric.Temple.VitricTempleWallSafe>();

					if (tile.TileType == Mod.Find<ModTile>("VitricSoftSand").Type)
						tile.TileType = Mod.Find<ModTile>("VitricSand").Type;
				}
			}

>>>>>>> 8ad5f156 (Update structures)
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