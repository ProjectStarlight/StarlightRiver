using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader.Default;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Starlight
{
	public class StarlightPylon : ModPylon
	{
		public const int CrystalVerticalFrameCount = 8;

		public override string Texture => AssetDirectory.StarlightTile + Name;

		public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.StyleHorizontal = true;

			TEModdedPylon moddedPylon = ModContent.GetInstance<StarlightPylonEntity>();
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(moddedPylon.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(moddedPylon.Hook_AfterPlacement, -1, 0, false);

			TileObjectData.addTile(Type);

			TileID.Sets.PreventsSandfall[Type] = true;
			TileID.Sets.AvoidedByMeteorLanding[Type] = true;

			AddToArray(ref TileID.Sets.CountsAsPylon);

			LocalizedText pylonName = CreateMapEntryName();
			AddMapEntry(Color.White, pylonName);
		}

		public override void MouseOver(int i, int j)
		{
			Main.LocalPlayer.cursorItemIconEnabled = true;
			Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<StarlightPylonItem>();
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			ModContent.GetInstance<StarlightPylonEntity>().Kill(i, j);
		}

		public override NPCShop.Entry GetNPCShopEntry()
		{
			return null;
		}

		public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount)
		{
			return true;
		}

		public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
		{
			return true;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.2f;
			g = 0.45f;
			b = 1f;
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			DefaultDrawPylonCrystal(spriteBatch, i, j, Assets.Tiles.Starlight.StarlightPylon_Crystal, Assets.Tiles.Starlight.StarlightPylon_CrystalHighlight, new Vector2(0f, -12f), Color.White * 0.1f, new Color(120, 200, 255), 4, CrystalVerticalFrameCount);
		}

		public override void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale)
		{
			bool mouseOver = DefaultDrawMapIcon(ref context, Assets.Tiles.Starlight.StarlightPylon_MapIcon, pylonInfo.PositionInTiles.ToVector2() + new Vector2(1.5f, 2f), drawColor, deselectedScale, selectedScale);
			DefaultMapClickHandle(mouseOver, pylonInfo, ModContent.GetInstance<StarlightPylonItem>().DisplayName.Key, ref mouseOverText);
		}
	}

	public sealed class StarlightPylonEntity : TEModdedPylon { }

	public sealed class StarlightPylonItem : QuickTileItem
	{
		public StarlightPylonItem() : base("Starlight Pylon", "You shouldn't have this!", "StarlightPylon", 0, AssetDirectory.Debug, true, 0) { }
	}
}