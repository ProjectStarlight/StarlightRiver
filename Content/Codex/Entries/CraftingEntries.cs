using StarlightRiver.Core;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Codex.Entries
{
    internal class StarlightWaterEntry : CodexEntry
    {
        public StarlightWaterEntry()
        {
            Category = Categories.Crafting;
            Title = "Starlight Bathing";
            Body = "You have discovered that if the stars that fall at night happen to fall into a body of water, they seem to disintegrate into a beautiful, shimmering light, which lingers on the water's surface. Submerging certain Items in this rather shiny water seems to alter them in strange ways, transforming things such as simple wooden tools into powerful magical instruments. NEWBLOCK " +
                "It also seems that anything capable of undergoing this metamorphasis appears to glow brightly in your pockets when near these sparkling waters. Building on this, you think it may be a wise idea to occasionally visit these wells and check if anything you have can be transformed.";
            Image = Request<Texture2D>("StarlightRiver/Assets/Codex/AbilityImageLore").Value;
            Icon = Request<Texture2D>("StarlightRiver/Assets/Codex/StarlightWaterIcon").Value;
        }
    }
}