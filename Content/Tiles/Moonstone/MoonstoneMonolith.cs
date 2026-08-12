using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Items.Moonstone;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.UI;

namespace StarlightRiver.Content.Tiles.Moonstone
{
	class MoonstoneMonolith : ModTile
	{
		public override string Texture => AssetDirectory.MoonstoneTile + Name;

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.StyleLineSkip = 2;//may need to be increased to add extra animation
													 //TileObjectData.newTile.StyleHorizontal = true;//may be needed if frame Y is changed
			QuickBlock.QuickSetFurniture(this, 2, 3,
				ModContent.DustType<MoonstoneArrowDust>(),
				SoundID.Tink, false,
				new Color(106, 113, 124), false, false,
				"Moonstone Monolith",
				new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0));
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;
			AnimationFrameHeight = 54;
			AdjTiles = new int[] { TileID.LunarMonolith };
			//RegisterItemDrop(ModContent.ItemType<MoonstoneMonolithItem>());//also works as a fix, may be needed instead if extra animation is added
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
		{
			return true;
		}

		public override void AnimateTile(ref int frame, ref int frameCounter)
		{
			//const int startframeOffset = 6;
			if ((frameCounter = ++frameCounter % 8) == 0)
				frame = ++frame % 12;
		}

		public override bool RightClick(int i, int j)
		{
			SoundEngine.PlaySound(SoundID.Mech, new Vector2(i, j) * 16);
			HitWire(i, j);
			return true;
		}

		public override void HitWire(int i, int j)
		{
			Tile interactTile = Main.tile[i, j];

			int offsetX = interactTile.TileFrameX / 18 % 2;
			int offsetY = interactTile.TileFrameY / 18 % 3;
			Tile targetTile = Main.tile[i - offsetX, j - offsetY];

			bool inactive = targetTile.TileFrameX == 0;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 0; y < 3; y++)
				{
					int coordX = i - offsetX + x;
					int coordY = j - offsetY + y;

					if (inactive)
						Main.tile[coordX, coordY].TileFrameX += 36;
					else
						Main.tile[coordX, coordY].TileFrameX = (short)(x * 18);

					if (Wiring.running)
						Wiring.SkipWire(coordX, coordY);
				}
			}

			NetMessage.SendTileSquare(-1, i - offsetX, j - offsetY + 1, 3);
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (Main.tile[i, j].TileFrameX >= 36)
				ModContent.GetInstance<MoonstoneBiomeSystem>().overrideVFXActive = true;
		}

		public override void MouseOver(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.noThrow = 2;
			Player.cursorItemIconID = ModContent.ItemType<MoonstoneMonolithItem>();
			Player.cursorItemIconEnabled = true;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Main.tile[i, j];
			Texture2D glowTexture = ModContent.Request<Texture2D>(AssetDirectory.MoonstoneTile + "MoonstoneMonolith_Glow").Value;

			Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
			//a
			const int height = 16;
			// if the bottom tile is a pixel taller
			//int height = tile.TileFrameY % AnimationFrameHeight == 36 ? 18 : 16;

			int frameYOffset = Main.tileFrame[Type] * AnimationFrameHeight;

			Vector2 pos = new Vector2(
					i * 16 - (int)Main.screenPosition.X,
					j * 16 - (int)Main.screenPosition.Y + 2) + zero;

			Rectangle frame = new Rectangle(
					tile.TileFrameX,
					tile.TileFrameY + frameYOffset,
					16,
					height);

			spriteBatch.Draw(
				glowTexture, pos, frame,
				Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			//Tile tile = Main.tile[i, j];
			//Texture2D glowTexture = ModContent.Request<Texture2D>(AssetDirectory.MoonstoneTile + "MoonstoneMonolith_Glow").Value;

			//Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
			////a
			//const int height = 16;
			//// if the bottom tile is a pixel taller
			////int height = tile.TileFrameY % AnimationFrameHeight == 36 ? 18 : 16;

			//int frameYOffset = Main.tileFrame[Type] * AnimationFrameHeight;

			//Vector2 pos = new Vector2(
			//		i * 16 - (int)Main.screenPosition.X,
			//		j * 16 - (int)Main.screenPosition.Y + 2) + zero;

			//Rectangle frame = new Rectangle(
			//		tile.TileFrameX,
			//		tile.TileFrameY + frameYOffset,
			//		16,
			//		height);

			//spriteBatch.Draw(
			//	glowTexture, pos, frame,
			//	Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			return true;
		}
	}

	public class MoonstoneMonolithItem : ModItem//having this as a accessory breaks QuickTileItem
	{
		public override string Texture => AssetDirectory.MoonstoneTile + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Moonstone Monolith");
			Tooltip.SetDefault("'Dreamifies the skies'");
		}

		public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 28;

			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(gold: 1);
			Item.vanity = true;
			Item.accessory = true;
			Item.maxStack = 9999;

			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.createTile = ModContent.TileType<MoonstoneMonolith>();
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<MoonstoneBarItem>(), 8);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			ModContent.GetInstance<MoonstoneBiomeSystem>().overrideVFXActive = true;
		}
	}
}