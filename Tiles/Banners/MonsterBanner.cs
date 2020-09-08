using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Banners
{
	public class MonsterBanner : ModTile
	{
		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.StyleWrapLimit = 18;//??? sprite width of one banner seems to work...
			//TileObjectData.newTile.StyleMultiplier = 1; //not needed since its one wide
			TileObjectData.addTile(Type);
			dustType = -1;
			disableSmartCursor = true;
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Banner");
			AddMapEntry(new Color(42, 88, 130), name);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			int style = frameX / 18;
			int item;
			switch (style) {
				case 0:
					item = ItemType<Items.Banners.VitricBatBanner>();
					break;
				case 1:
					item = ItemType<Items.Banners.VitricSlimeBanner>();
					break;
				case 2:
					item = ItemType<Items.Banners.OvergrowthNightmareBanner>();
					break;
				case 3:
					item = ItemType<Items.Banners.CorruptHornetBanner>();
					break;
				case 4:
					item = ItemType<Items.Banners.OvergrowthSkeletonBanner>();
					break;
				default:
					return;
			}
			Item.NewItem(i * 16, j * 16, 16, 48, item);
		}

		public override void NearbyEffects(int i, int j, bool closer) {
			if (closer) {
				Player player = Main.LocalPlayer;
				int style = Main.tile[i, j].frameX / 18;
				int type;
				switch (style) {
					case 0:
						type = NPCType<NPCs.Hostile.CrystalPopper>();
						break;
					case 1:
						type = NPCType<NPCs.Hostile.CrystalSlime>();
						break;
					case 2:
						type = NPCType<NPCs.Hostile.OvergrowNightmare>();
						break;
					case 3:
						type = NPCType<NPCs.Hostile.JungleCorruptWasp>();
						break;
					case 4:
						type = NPCType<NPCs.Hostile.OvergrowSkeletonLarge>();
						break;
					default:
						return;
				}
				player.NPCBannerBuff[type] = true;
				player.hasBanner = true;
			}
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			if (i % 2 == 1) {
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
		}
	}
}
