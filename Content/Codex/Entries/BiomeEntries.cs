using StarlightRiver.Helpers;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Codex.Entries
{
	internal class PermafrostEntry : CodexEntry
    {
        public PermafrostEntry()
        {
            Category = Categories.Biomes;
            Title = "The Permafrost";
            Body = Helper.WrapString("No text",
                500, Main.fontDeathText, 0.8f);
            Hint = "Found beneath the icy depths...";
            Image = GetTexture("StarlightRiver/Assets/Codex/BiomeImageAurora");
            Icon = GetTexture("StarlightRiver/Assets/Codex/BiomeIconAurora");
        }
    }

    internal class VitricEntry : CodexEntry
    {
        public VitricEntry()
        {
            Category = Categories.Biomes;
            Title = "Vitric Desert";
            Body = Helper.WrapString("No text",
                500, Main.fontDeathText, 0.8f);
            Hint = "Found beneath the underground desert...";
            Image = GetTexture("StarlightRiver/Assets/Codex/BiomeImageVitric");
            Icon = GetTexture("StarlightRiver/Assets/Codex/BiomeIconVitric");
        }
    }

    internal class OvergrowEntry : CodexEntry
    {
        public OvergrowEntry()
        {
            Category = Categories.Biomes;
            Title = "The Overgrowth";
            Body = Helper.WrapString("No text",
                500, Main.fontDeathText, 0.8f);
            Hint = "Found beyond a sealed door in the dungeon...";
            Image = GetTexture("StarlightRiver/Assets/Codex/BiomeImageOvergrow");
            Icon = GetTexture("StarlightRiver/Assets/Codex/BiomeIconOvergrow");
        }
    }
}