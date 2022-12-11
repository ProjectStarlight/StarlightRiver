using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	internal class VitricPick : ModItem, IGlowingItem
	{
		public int heat = 0;
		public int heatTime = 0;

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void Load()
		{
			On.Terraria.Player.PickTile += GenerateHeat;
		}

		public override void Unload()
		{
			On.Terraria.Player.PickTile -= GenerateHeat;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vitric Pickaxe");
			Tooltip.SetDefault("Hellstone does not drop lava\nMining hellstone generates heat\n Heat increases speed");
		}

		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.DamageType = DamageClass.Melee;
			Item.width = 38;
			Item.height = 38;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.pick = 85;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5f;
			Item.value = 1000;
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item18;
			Item.useTurn = true;
		}

		private void GenerateHeat(On.Terraria.Player.orig_PickTile orig, Player self, int x, int y, int pickPower)
		{
			var myPick = self.HeldItem.ModItem as VitricPick;
			Tile tile = Framing.GetTileSafely(x, y);
			ushort type = tile.TileType;

			orig(self, x, y, pickPower);

			tile = Framing.GetTileSafely(x, y);

			if (myPick != null && type == TileID.Hellstone)
			{
				if (myPick.heat < 20)
					myPick.heat++;

				tile.LiquidType = 0;
				tile.LiquidAmount = 0;
				tile.SkipLiquid = true;
				NetMessage.SendTileSquare(0, x, y, 1, 1);
			}
		}

		public override float UseTimeMultiplier(Player Player)
		{
			return 1 + heat / 40f;
		}

		public override void UpdateInventory(Player Player)
		{
			heatTime++;

			if (heatTime >= 40)
			{
				if (heat > 0)
					heat--;

				heatTime = 0;
			}
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			Texture2D tex = Request<Texture2D>(Texture + "Glow").Value;
			Color color = Color.White * (heat / 20f);

			spriteBatch.Draw(tex, position, frame, color, 0, origin, scale, 0, 0);
		}

		public void DrawGlowmask(PlayerDrawSet info)
		{
			Player Player = info.drawPlayer;

			if (Player.itemAnimation == 0)
				return;

			Texture2D tex = Request<Texture2D>(Texture + "Glow").Value;
			Color color = Color.White * (heat / 20f);
			Vector2 origin = Player.direction == 1 ? new Vector2(0, tex.Height) : new Vector2(tex.Width, tex.Height);

			var data = new DrawData(tex, info.ItemLocation - Main.screenPosition, null, color, Player.itemRotation, origin, Item.scale, Player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			info.DrawDataCache.Add(data);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<SandstoneChunk>(), 10);
			recipe.AddIngredient(ItemType<VitricOre>(), 20);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}