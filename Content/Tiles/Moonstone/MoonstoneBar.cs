using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Moonstone
{
	public class MoonstoneBar : ModTile
    {
        public override bool Autoload(ref string name, ref string texture) {
            texture = AssetDirectory.MoonstoneTile + name;
            return base.Autoload(ref name, ref texture); }

        public override void SetDefaults() =>
            this.QuickSetBar(ItemType<Items.Moonstone.MoonstoneBar>(), DustType<Dusts.Electric>(), new Color(156, 172, 177));
    }
}