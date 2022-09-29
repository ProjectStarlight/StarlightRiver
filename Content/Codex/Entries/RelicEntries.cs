using StarlightRiver.Core;
using Microsoft.Xna.Framework.Graphics;
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
            Image = Request<Texture2D>("StarlightRiver/Assets/Codex/AbilityImageLore").Value;
            Icon = Request<Texture2D>("StarlightRiver/Assets/Codex/StarlightWaterIcon").Value;
        }
    }
}