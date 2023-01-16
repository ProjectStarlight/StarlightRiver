using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Moonstone
{
	public class MoonstoneBar : ModTile
    {
        public override string Texture => AssetDirectory.MoonstoneTile + Name;

        public override void SetStaticDefaults() =>
            this.QuickSetBar(ItemType<Items.Moonstone.MoonstoneBarItem>(), DustType<Dusts.Electric>(), new Color(156, 172, 177));
    }
}