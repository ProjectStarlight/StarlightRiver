using System;

namespace StarlightRiver.Content.WorldGeneration
{
	[Flags]
    public enum ChestRegionFlags
    {
        All = 0b1, // Applies to any of these listed chests.
        Surface = 0b10, // Wooden chests.
        Underground = 0b100, // Underground gold chests.
        Ice = 0b1000,
        Jungle = 0b10000, // Not including the temple.
        Temple = 0b100000,
        Sky = 0b1000000,
        Underwater = 0b10000000,
        Spider = 0b100000000,
        Granite = 0b1000000000,
        Marble = 0b10000000000,
        Underworld = 0b100000000000,
        Dungeon = 0b1000000000000 // Only locked gold chests - biome chests should not have their Items replaced.
    }
}
