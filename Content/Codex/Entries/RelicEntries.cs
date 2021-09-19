using StarlightRiver.Core;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Codex.Entries
{
    internal class PlaceholderRelicEntry : CodexEntry
    {
        public PlaceholderRelicEntry()
        {
            Category = Categories.Relics;
            Title = "Placeholder Entry";
            Body = "Huh. Well this is awkward."; 
            Image = GetTexture("StarlightRiver/Assets/Codex/AbilityImageLore");
            Icon = GetTexture("StarlightRiver/Assets/Codex/StarlightWaterIcon");
        }
    }
}