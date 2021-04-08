using System;

namespace StarlightRiver.Content.WorldGeneration
{
    [Flags]
    public enum ChestRegionFlags
    {
        All = 1, // Applies to any of these listed chests.
        Surface = 2, // Wooden chests.
        Underground = 4, // Underground gold chests.
        Ice = 8,
        Jungle = 16, // Not including the temple.
        Temple = 32,
        Sky = 64,
        Underwater = 128,
        Spider = 256,
        Granite = 512,
        Marble = 1024,
        Underworld = 2048,
        Dungeon = 4096 // Only locked gold chests - biome chests should not have their items replaced.
    }
}
