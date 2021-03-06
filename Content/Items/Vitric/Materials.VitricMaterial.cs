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

        public SandstoneChunk() : base("Ceramic Chunk", "", 999, 200, 2) { }
    }
    public class MagmaCore : QuickMaterial
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public MagmaCore() : base("Magma Core", "", 999, 200, 2) { }
    }
}