using StarlightRiver.Helpers;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Codex.Entries
{
    internal class VitricEntry : CodexEntry
    {
        public VitricEntry()
        {
            Category = Categories.Biomes;
            Title = "Vitric Desert";
            Body = Helper.WrapString("",
                500, Main.fontDeathText, 0.8f);
            Hint = "Found beneath the underground desert...";
            Image = GetTexture("StarlightRiver/Assets/Codex/BiomeImageVitric");
            Icon = GetTexture("StarlightRiver/Assets/Codex/BiomeIconVitric");
        }
    }
}