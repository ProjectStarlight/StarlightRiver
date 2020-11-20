using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Fountains
{
    public class JungleCorruptFountainItem : QuickTileItem
    {
        public JungleCorruptFountainItem() : base("Corrupt Jungle Fountain", "", TileType<Tiles.Fountains.JungleCorruptFountain>(), 0)
        {
        }
    }

    public class JungleBloodyFountainItem : QuickTileItem
    {
        public JungleBloodyFountainItem() : base("Crimson Jungle Fountain", "", TileType<Tiles.Fountains.JungleBloodyFountain>(), 0)
        {
        }
    }

    public class JungleHolyFountainItem : QuickTileItem
    {
        public JungleHolyFountainItem() : base("Hallow Jungle Fountain", "", TileType<Tiles.Fountains.JungleHolyFountain>(), 0)
        {
        }
    }
}