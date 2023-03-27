using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Helpers;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Desert
{
	public class AnkhChest : DummyTile
	{
		public override string Texture => AssetDirectory.DesertTile + Name;

		public override int DummyType => ModContent.ProjectileType<AnkhChestDummy>();

		public override void SetStaticDefaults()
		{
			Main.tileSpelunker[Type] = true;
			Main.tileContainer[Type] = true;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 1200;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileOreFinderPriority[Type] = 500;

			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.BasicChest[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.CoordinateHeights = new int[] { 16, 18 };
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
			TileObjectData.newTile.AnchorInvalidTiles = new int[] { 127 };
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.addTile(Type);

			ModTranslation name = CreateMapEntryName();
			AddMapEntry(Color.Gold, name, MapChestName);
			name = CreateMapEntryName(Name + "_Locked");
			name.SetDefault("Ankh Chest");
			AddMapEntry(Color.Cyan, name, MapChestName);
			DustType = DustID.Sand;
			AdjTiles = new int[] { TileID.Containers };
			ChestDrop = ModContent.ItemType<AnkhChestItem>();
			ContainerName.SetDefault("Ankh Chest");
		}

		public string MapChestName(string name, int i, int j)
		{
			int left = i;
			int top = j;
			Tile tile = Main.tile[i, j];

			if (tile.TileFrameX % 36 != 0)
				left--;

			if (tile.TileFrameY != 0)
				top--;

			int chest = Chest.FindChest(left, top);

			if (chest < 0)
				return Language.GetTextValue("LegacyChestType.0");
			else if (Main.chest[chest].name == "")
				return name;
			else
				return name + ": " + Main.chest[chest].name;
		}

		public override bool SpawnConditions(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			return tile.TileFrameX == 72 && tile.TileFrameY == 0;
		}

		public override ushort GetMapOption(int i, int j)
		{
			int style = Main.tile[i, j].TileFrameX / 36;

			// MapOption don't match up with styles, since 0 and 2 were used.
			return (ushort)(style == 0 ? 0 : 1);
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
		{
			return true;
		}

		public override bool IsLockedChest(int i, int j)
		{
			return Main.tile[i, j].TileFrameX / 36 == 2;
		}

		public override bool UnlockChest(int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual)
		{
			frameXAdjustment = -72;
			return true;
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = 1;
		}

		public override bool CanKillTile(int i, int j, ref bool blockDamaged)
		{
			Tile tile = Main.tile[i, j];
			int left = i;
			int top = j;

			if (tile.TileFrameX % 36 != 0)
				left--;

			if (tile.TileFrameY != 0)
				top--;

			return Chest.CanDestroyChest(left, top);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, 32, 32, ChestDrop);
			Chest.DestroyChest(i, j);
		}

		public override bool RightClick(int i, int j)
		{
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			Main.mouseRightRelease = false;
			int left = i;
			int top = j;

			if (tile.TileFrameX % 36 != 0)
				left--;

			if (tile.TileFrameY != 0)
				top--;

			if (player.sign >= 0)
			{
				SoundEngine.PlaySound(SoundID.MenuClose);
				player.sign = -1;
				Main.editSign = false;
				Main.npcChatText = "";
			}

			if (Main.editChest)
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
				Main.editChest = false;
				Main.npcChatText = "";
			}

			if (player.editedChestName)
			{
				NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f, 0f, 0f, 0, 0, 0);
				player.editedChestName = false;
			}

			bool isLocked = IsLockedChest(left, top);

			if (Main.netMode == NetmodeID.MultiplayerClient && !isLocked)
			{
				if (left == player.chestX && top == player.chestY && player.chest >= 0)
				{
					player.chest = -1;
					Recipe.FindRecipes();
					SoundEngine.PlaySound(SoundID.MenuClose);
				}
				else
				{
					NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, top, 0f, 0f, 0, 0, 0);
					Main.stackSplit = 600;
				}
			}
			else
			{
				if (!isLocked)
				{
					int chest = Chest.FindChest(left, top);

					if (chest >= 0)
					{
						Main.stackSplit = 600;

						if (chest == player.chest)
						{
							player.chest = -1;
							SoundEngine.PlaySound(SoundID.MenuClose);
						}
						else
						{
							player.chest = chest;
							Main.playerInventory = true;
							Main.recBigList = false;
							player.chestX = left;
							player.chestY = top;
							SoundEngine.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
						}

						Recipe.FindRecipes();
					}
				}
			}

			return true;
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			int left = i;
			int top = j;

			if (tile.TileFrameX % 36 != 0)
				left--;

			if (tile.TileFrameY != 0)
				top--;

			int chest = Chest.FindChest(left, top);

			if (chest >= 0 && tile.TileFrameX < 72)
			{
				player.cursorItemIconID = ChestDrop;
				player.cursorItemIconText = "";
				player.noThrow = 2;
				player.cursorItemIconEnabled = true;
			}
		}
	}

	internal class AnkhChestDummy : Dummy
	{
		public override string Texture => AssetDirectory.DesertTile + "AnkhChestGlow";

		public AnkhChestDummy() : base(ModContent.TileType<AnkhChest>(), 32, 48) { }

		public override void Collision(Player Player)
		{
			if (AbilityHelper.CheckDash(Player, Projectile.Hitbox))
			{
				SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
				Player.GetHandler().ActiveAbility?.Deactivate();
				Player.velocity = Vector2.Normalize(Player.velocity) * -10f;

				int i = (int)(Projectile.position.X / 16);
				int j = (int)(Projectile.position.Y / 16);

				for (int k = 0; k < 2; k++)
				{
					for (int l = 0; l < 2; l++)
					{
						Tile tile = Framing.GetTileSafely(i + k, j + l);
						tile.TileFrameX -= 72;
					}
				}

				for (int k = 0; k <= 10; k++)
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.3f);
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Air>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 0.8f);
				}

				Projectile.active = false;
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Color color = Helper.IndicatorColorProximity(150, 300, Projectile.Center);

			Main.spriteBatch.Draw(tex, Projectile.position - new Vector2(1, -1) - Main.screenPosition, color);
		}
	}

	class AnkhChestItem : QuickTileItem
	{
		public AnkhChestItem() : base("Ankh Chest", "", "AnkhChest", 1, AssetDirectory.DesertTile, false) { }
	}
}