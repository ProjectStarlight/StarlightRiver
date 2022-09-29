using StarlightRiver.Core;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Codex.Entries
{
	internal class LoreEntry : CodexEntry
    {
        public LoreEntry()
        {
            Category = Categories.Abilities;
            Title = "Starlight Codex";
            Body = "A mysterious compendium containing lost knowledge, it seems to write itself as you travel. Click the codex icon in your inventory to view the codex.";
            Image = Request<Texture2D>("StarlightRiver/Assets/Codex/AbilityImageLore").Value;
            Icon = Request<Texture2D>("StarlightRiver/Assets/GUI/Book1Closed").Value;
        }
    }

    internal class WindsEntry : CodexEntry
    {
        public WindsEntry()
        {
            Category = Categories.Abilities;
            Title = "Forbidden Winds";
            Body = "A collection of strange energies found deeep within a tomb buried in the vitric desert. These 'winds' hold the power to shatter certain objects on touch and propel you forward at great speeds.";
            Hint = "Sealed away in an ancient glass temple";
            Image = Request<Texture2D>(AssetDirectory.Debug).Value;
            Icon = Request<Texture2D>("StarlightRiver/Assets/Abilities/ForbiddenWinds").Value;
        }
    }
}