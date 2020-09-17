using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Items
{
    internal interface IChestItem
    {
        int ItemStack();
        bool GenerateCondition(Chest chest);
    }
}