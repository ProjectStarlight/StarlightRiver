using StarlightRiver.Helpers;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Misc
{
	public class RatTent : ModTile
	{
		public override string Texture => AssetDirectory.MiscTile + Name;

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.DrawYOffset = 2;
			this.QuickSetFurniture(5, 4, 26, SoundID.Dig, false, new Color(163, 161, 96), false, false, "Strange Tent");
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = 1;
		}

		public override bool RightClick(int i, int j)
		{
			Vector2 vel = (-Vector2.UnitY * 8).RotatedBy(Main.rand.NextFloat(-1f, 1f));
			Main.npc[NPC.NewNPC(new EntitySource_WorldEvent(), i * 16, j * 16, Main.rand.Next(350) == 0 ? NPCID.GoldMouse : NPCID.Mouse)].velocity = vel;

			Helper.PlayPitched(SoundID.NPCDeath4, 0.3f, Main.rand.NextFloat(-0.1f, 0.1f));
			return true;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ItemType<RatTentItem>());
		}
	}

	public class RatTentItem : QuickTileItem
	{
		public RatTentItem() : base("Strange Tent", "Whats inside?...", "RatTent", 1, AssetDirectory.MiscTile) { }
	}
}