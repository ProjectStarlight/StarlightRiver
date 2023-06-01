using StarlightRiver.Core.Systems;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	class ForgeDoor : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			MinPick = int.MaxValue;
			TileID.Sets.DrawsWalls[Type] = true;
			QuickBlock.QuickSetFurniture(this, 2, 6, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(200, 150, 80), false, true, "Forge Door");
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			tile.IsActuated = !NPC.AnyNPCs(NPCType<Bosses.GlassMiniboss.Glassweaver>());
		}
	}

	[SLRDebug]
	class ForgeDoorItem : QuickTileItem
	{
		public ForgeDoorItem() : base("Forge Door", "Debug Item", "ForgeDoor", 1, AssetDirectory.Debug, true) { }
	}
}