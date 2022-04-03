using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Items.Herbology.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Crafting
{
    internal class Infuser : ModTile
    {
        public override string Texture => AssetDirectory.CraftingTile + Name;

        public override void SetDefaults() => this.QuickSetFurniture(4, 4, DustID.Stone, SoundID.Dig, false, new Color(113, 113, 113), false, false, "Infuser");

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<OvenItem>());

		public override bool NewRightClick(int i, int j)
		{
            InfusionMaker.visible = true;
            return true;
        }
	}

    public class InfuserItem : QuickTileItem
    {
        public InfuserItem() : base("[PH]Infuser", "Used to imprint infusions", TileType<Infuser>(), 0, AssetDirectory.CraftingTile) { }
    }
}
