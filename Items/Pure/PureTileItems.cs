using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Pure
{
    public class StonePureItem : QuickTileItem
    {
        public StonePureItem() : base("Purestone", "It shines brilliantly", TileType<Tiles.Purified.StonePure2>(), 0)
        {
        }
    }
}