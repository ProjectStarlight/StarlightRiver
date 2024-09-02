﻿using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class DestructableGlass : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricGlass";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSet(this, int.MaxValue, ModContent.DustType<Dusts.GlassGravity>(), SoundID.Shatter, new Color(100, 150, 255), ModContent.ItemType<Items.Vitric.VitricOre>());

			TileID.Sets.DrawsWalls[Type] = true;
			Main.tileBlockLight[Type] = false;

			ushort sandstone = Mod.Find<ModTile>("AncientSandstone").Type;
			Main.tileMerge[Type][sandstone] = true;
			Main.tileMerge[sandstone][Type] = true;

			RegisterItemDrop(ModContent.ItemType<Items.Vitric.VitricOre>(), 0);
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			//NearbyEffects is clientside only
			if (closer)
			{
				var hitbox = new Rectangle(i * 16, j * 16, 16, 16);

				int speed = (int)Main.LocalPlayer.velocity.Length() * 2;

				if (speed < 32)
					speed = 32;

				hitbox.Inflate(speed, speed);

				bool colliding = Abilities.AbilityHelper.CheckDash(Main.LocalPlayer, hitbox);

				if (colliding)
				{
					WorldGen.KillTile(i, j);

					// For a multplayer client, killTile doesn't spawn items so we'll spawn item ourselves and send a packet
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						int item = Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ModContent.ItemType<Items.Vitric.VitricOre>(), 1);

						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
					}

					Framing.GetTileSafely(i, j).IsActuated = true;
					NetMessage.SendTileSquare(Main.LocalPlayer.whoAmI, i, j);
				}
				else
				{
					Framing.GetTileSafely(i, j).IsActuated = false;
				}
			}
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			Color color = Helpers.Helper.IndicatorColorProximity(64, 128, new Vector2(i, j) * 16 + Vector2.One * 8);

			Texture2D tex = Assets.Tiles.Vitric.VitricGlassGlow.Value;

			spriteBatch.Draw(tex, (new Vector2(i, j) + Helpers.Helper.TileAdj) * 16 - Main.screenPosition, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), color);
		}
	}

	internal class DestructableGlassItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricGlassItem";

		public DestructableGlassItem() : base("Breakable vitric crystal", "A chunk of vitric crystal susceptible to the force of forbidden winds", "DestructableGlass", 2) { }

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<Items.Vitric.VitricOre>(), 1);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}