using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Accessories.EarlyPreHardmode
{
    public class SojournersSash : SmartAccessory, IChestItem
    {
        public SojournersSash() : base("Sojourner's Sash", "20% increased max movement speed, with halved mana regeneration while moving") { }

        public override void SafeUpdateEquip(Player player)
        {
            player.maxRunSpeed *= 1.20f;
            if (player.velocity.Length() > 0.5)
                player.manaRegen = (int)((float)player.manaRegen / 2f);
        }

        public int ItemStack() => 1;
        public bool GenerateCondition(Chest chest)
        {
            return true;
        }

    }
}