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
            Image = GetTexture("StarlightRiver/Codex/Entries/AbilityImageLore");
            Icon = GetTexture("StarlightRiver/GUI/Assets/Book1Closed");
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
            Image = GetTexture("StarlightRiver/MarioCumming");
            Icon = GetTexture("StarlightRiver/Pickups/ForbiddenWinds");
        }
    }

    internal class FaeEntry : CodexEntry
    {
        public FaeEntry()
        {
            Category = Categories.Abilities;
            Title = "Faeflame";
            Body = "NO TEXT";
            Hint = "Found upon an altar in an overgrown dungeon";
            Image = GetTexture("StarlightRiver/Codex/Entries/AbilityImageWisp");
            Icon = GetTexture("StarlightRiver/Pickups/Faeflame");
        }
    }

    internal class PureEntry : CodexEntry
    {
        public PureEntry()
        {
            Category = Categories.Abilities;
            Title = "Corona of Purity";
            Body = "NO TEXT";
            Hint = "Found in a temple at the brink of the world...";
            Image = GetTexture("StarlightRiver/MarioCumming");
            Icon = GetTexture("StarlightRiver/Pickups/PureCrown");
        }
    }

    internal class SmashEntry : CodexEntry
    {
        public SmashEntry()
        {
            Category = Categories.Abilities;
            Title = "Gaia's Fist";
            Body = "NO TEXT";
            Hint = "PENIS";
            Image = GetTexture("StarlightRiver/MarioCumming");
            Icon = GetTexture("StarlightRiver/Pickups/GaiaFist");
        }
    }

    internal class LoreEntry2 : CodexEntry
    {
        public LoreEntry2()
        {
            Category = Categories.Abilities;
            Title = "Rift Codex";
            Body = "NO TEXT";
            Image = GetTexture("StarlightRiver/GUI/Assets/Book2Closed");
            Icon = GetTexture("StarlightRiver/GUI/Assets/Book2Closed");
            RequiresUpgradedBook = true;
        }
    }

    internal class CloakEntry : CodexEntry
    {
        public CloakEntry()
        {
            Category = Categories.Abilities;
            Title = "Zzelera's Cloak";
            Body = "NO TEXT";
            Hint = "PENIS";
            Image = GetTexture("StarlightRiver/MarioCumming");
            Icon = GetTexture("StarlightRiver/Pickups/Cloak1");
            RequiresUpgradedBook = true;
        }
    }
}