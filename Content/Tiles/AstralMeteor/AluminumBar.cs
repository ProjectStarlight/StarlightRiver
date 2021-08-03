using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.AstralMeteor
{
	public class AluminumBar : ModTile
    {
        public override bool Autoload(ref string name, ref string texture) {
            texture = AssetDirectory.AluminumTile + name;
            return base.Autoload(ref name, ref texture); }

        public override void SetDefaults() =>
            this.QuickSetBar(ItemType<Items.AstralMeteor.AluminumBar>(), DustType<Dusts.Electric>(), new Color(156, 172, 177));
    }
}