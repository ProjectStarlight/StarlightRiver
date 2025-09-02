using System;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	internal class VitricPick : ModItem, IGlowingItem
	{
		public const int MAX_HEAT = 20;

		public static Point16 lastVitricPickInteraction;

		public int heat = 0;
		public int heatTime = 0;

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void Load()
		{
			On_Player.PickTile += GenerateHeat;
			On_NetMessage.TrySendData += SendPickInfo;
			On_WorldGen.KillTile += CancelLava;
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

		private void GenerateHeat(On_Player.orig_PickTile orig, Player self, int x, int y, int pickPower)
		{
			Tile tile = Framing.GetTileSafely(x, y);
			ushort oldType = tile.TileType;

			orig(self, x, y, pickPower);

			var myPick = self.HeldItem.ModItem as VitricPick;

			if (myPick != null && oldType == TileID.Hellstone)
			{
				if (myPick.heat < MAX_HEAT)
					myPick.heat++;

				// Sets for singleplayer, for multiplayer see SendPickInfo to set this on the server based on picktile packets
				lastVitricPickInteraction = new(x, y);
			}
			else
			{
				lastVitricPickInteraction = default;
			}
		}

		private bool SendPickInfo(On_NetMessage.orig_TrySendData orig, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
		{
			bool final = orig(msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);

			if (Main.netMode == NetmodeID.Server && msgType == MessageID.SyncTilePicking && ignoreClient != -1)
			{
				Player player = Main.player[ignoreClient];

				if (player.HeldItem.type == ModContent.ItemType<VitricPick>())
					lastVitricPickInteraction = new((int)number2, (int)number3); // These are the X and Y coordinates, see vanilla MessageBuffer.cs for case 125 (tile pick packet)
			}

			return final;
		}

		private void CancelLava(On_WorldGen.orig_KillTile orig, int i, int j, bool fail, bool effectOnly, bool noItem)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			ushort oldType = tile.TileType;

			orig(i, j, fail, effectOnly, noItem);

			if (lastVitricPickInteraction == new Point16(i, j) && oldType == TileID.Hellstone)
			{
				tile.LiquidType = 0;
				tile.LiquidAmount = 0;
				tile.SkipLiquid = true;
			}
		}

		public override float UseTimeMultiplier(Player Player)
		{
			return 1 - heat / (float)(MAX_HEAT * 2);
		}

		public override float UseAnimationMultiplier(Player player)
		{
			return 1 - heat / (float)(MAX_HEAT * 2);
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
			Color color = Color.White * (heat / (float)MAX_HEAT);

			spriteBatch.Draw(tex, position, frame, color, 0, origin, scale, 0, 0);
		}

		public void DrawGlowmask(PlayerDrawSet info)
		{
			Player Player = info.drawPlayer;

			if (Player.itemAnimation == 0)
				return;

			Texture2D tex = Request<Texture2D>(Texture + "Glow").Value;
			Color color = Color.White * (heat / (float)MAX_HEAT);
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