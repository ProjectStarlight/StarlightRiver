using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Codex.Entries
{
    internal class StaminaEntry : CodexEntry
    {
        public StaminaEntry()
        {
            Category = Categories.Misc;
            Title = "Stamina";
            Body = "All of your abilities utilize stamina, a resource represented by the orange diamonds to the right of your mana bar. Your stamina will replenish itself over time. You can gain additional stamina by unlocking new abilities and equipping certain armors and accessories.";
            Hint = "Unlock an ability...";
            Image = GetTexture("StarlightRiver/GUI/Assets/Stamina");
            Icon = GetTexture("StarlightRiver/GUI/Assets/Stamina");
        }
    }

    internal class StaminaShardEntry : CodexEntry
    {
        public StaminaShardEntry()
        {
            Category = Categories.Misc;
            Title = "Stamina Vessels";
            Body = "Shards of these intricate metal devices seem to be scattered around the world everywhere. By combining 3 of them, a completed stamina vessel can be made, allowing it's holder to store an extra unit of stamina. Unfortunately, the strange material and intricate tooling of these fragments makes it nigh impossible to re-create them on your own.";
            Hint = "Find a stamina vessel shard...";
            Image = GetTexture("StarlightRiver/GUI/Assets/StaminaEmpty");
            Icon = GetTexture("StarlightRiver/Pickups/Stamina1");
        }
    }

    internal class InfusionEntry : CodexEntry
    {
        public InfusionEntry()
        {
            Category = Categories.Misc;
            Title = "Infusions";
            Body = "";
            Hint = "Find a mysterious altar...";
            Image = GetTexture("StarlightRiver/Items/Infusions/DashAstralItem");
            Icon = GetTexture("StarlightRiver/Items/Infusions/DashAstralItem");
        }
    }
}