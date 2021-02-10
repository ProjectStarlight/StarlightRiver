using Terraria;

using StarlightRiver.Core;

namespace StarlightRiver.Items
{
    internal interface IChestItem
    {
        int ItemStack(Chest chest);
        bool GenerateCondition(Chest chest);
    }
}