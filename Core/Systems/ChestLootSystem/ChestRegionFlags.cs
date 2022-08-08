using System;

namespace StarlightRiver.Core.Systems.ChestLootSystem
{
    [Flags]
    public enum ChestRegionFlags
    {
        All = 0b1, // Applies to any of these listed chests.
        Surface = 0b10, // Wooden chests.
        Underground = 0b100, // Underground gold chests.
        Ice = 0b1000,
        Livingwood = 0b10000, // Jungle shrines and large jungle trees
        Desert = 0b100000,
        Sky = 0b1000000,
        Underwater = 0b10000000,
        Spider = 0b100000000,
        Granite = 0b1000000000,
        Marble = 0b10000000000,
        Underworld = 0b100000000000,
        Dungeon = 0b1000000000000, // Only locked gold chests
        Biome = 0b10000000000000, // will not effect desert biome chests
        Jungle = 0b100000000000000,
        JungleShrine = 0b1000000000000000,
        Temple = 0b10000000000000000,
        Mushroom = 0b100000000000000000,
        TrappedUnderground = 0b1000000000000000000,
        Barrel = 0b10000000000000000000,
        Trashcan = 0b100000000000000000000,
        Vitric = 0b1000000000000000000000,
        Permafrost = 0b10000000000000000000000,
        Overgrowth = 0b100000000000000000000000,
    }
}
