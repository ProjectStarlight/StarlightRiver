using StarlightRiver.Content.GUI;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Crafting
{
	internal class Infuser : ModTile
	{
		public override string Texture => AssetDirectory.CraftingTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSetFurniture(4, 4, DustID.Stone, SoundID.Dig, false, new Color(113, 113, 113), false, false, "Infuser");
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ItemID.DirtBlock);
		}

		public override bool RightClick(int i, int j)
		{
			InfusionMaker.visible = true;
			return true;
		}
	}

	public class InfuserItem : QuickTileItem
	{
		public InfuserItem() : base("[PH]Infuser", "Used to imprint infusions", "Infuser", 0, AssetDirectory.CraftingTile) { }
	}
}