using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Systems;
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
	}

	[SLRDebug]
	public class InfuserItem : QuickTileItem
	{
		public InfuserItem() : base("[PH]Infuser", "Used to imprint infusions", "Infuser", 0, AssetDirectory.CraftingTile) { }
	}
}