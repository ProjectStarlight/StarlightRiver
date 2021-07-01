using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

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