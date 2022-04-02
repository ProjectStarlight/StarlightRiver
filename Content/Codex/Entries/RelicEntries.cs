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
            Hint = "Some things you just cant know in a demo...";
            Image = GetTexture("StarlightRiver/Assets/Codex/AbilityImageLore");
            Icon = GetTexture("StarlightRiver/Assets/Codex/StarlightWaterIcon");
        }
    }
}