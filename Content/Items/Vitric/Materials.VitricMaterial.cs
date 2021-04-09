using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Vitric
{
    public class VitricOre : QuickMaterial
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public VitricOre() : base("Vitric Sliver", "", 999, 200, 2) { }
    }

    public class SandstoneChunk : QuickMaterial
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public SandstoneChunk() : base("Ancient Sandstone", "", 999, 200, 2) { }
    }
}